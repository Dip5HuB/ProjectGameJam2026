using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // DAFTAR STATE
    public enum PlayerState
    {
        Idle,
        Scary,
        Move,
        Airborne,
        Attack,
        Dash,
        Stealth,
        Stagger,
        Dead
    }

    public enum BuffType 
    { 
        None, 
        Haste, 
        Berserk, 
        Aegis 
    }

    [Header("Current State")]
    [SerializeField] private PlayerState currentState = PlayerState.Idle;

    [Header("Combat & Mechanics")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private float baseAttackDuration = 1f;
    private float currentAttackDuration; // Durasi serangan yang bisa disesuaikan dengan multiplier
    private bool isAttackBuffered = false; // Menyimpan antrean klik selanjutnya
    [SerializeField] private float attackSpeedMultiplier = 1f;
    [SerializeField] private float staggerDuration = 0.4f;
    [SerializeField] private Transform attackPoint;      
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float comboResetDelay = 2.5f; // Batas waktu toleransi jeda klik antar combo
    private float comboResetTimer;
    [SerializeField] private LayerMask enemyLayer; // Layer khusus untuk mendeteksi Musuh
    [SerializeField] private int enemyContactDamage = 15; // Damage saat menyenggol musuh
    [SerializeField] private float iFrameDuration = 0.8f;   // Durasi kebal setelah kena hit (0.8 detik)
    private float iFrameTimer; // Menghitung mundur sisa waktu kebal

    [Header("Buff & Drop System Settings")]
    [SerializeField] private BuffType activeBuff = BuffType.None;
    private float buffTimer;
    private float baseMoveSpeed; // Menampung speed asli sebelum dikali sandal swallow
    private bool isInvincible = false; // Penanda efek kebal peci haji

    [Header("Animation Cancelling Settings")]
    [SerializeField] private float dashCancelThreshold = 0.6f;    // Dash bisa cancel Attack setelah 60% animasi
    [SerializeField] private float staggerCancelThreshold = 0.7f; // Bisa cancel Stagger setelah 70% animasi

    [Header("Movement Settings")]
    [SerializeField] private float timeToTurnScary = 10f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f; // Durasi cooldown dalam detik
    [SerializeField] private float stealthDuration = 6f;
    [SerializeField] private float stealthCooldown = 10f; // Durasi cooldown 
    private float stealthCooldownTimer;
    private bool canStealth = true;
    private float dashCooldownTimer; // Timer yang akan menghitung mundur
    private bool canDash = true; // Penanda status boleh dash
    private float afkTimer; // Penghitung mundur waktu diam pemain

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    // Komponen Internal
    private Rigidbody2D rb;
    private Animator anim;
    private GameInput gameInput;
    
    // Variabel Pendukung Logika
    private float horizontalInput;
    private bool isGrounded;
    private float stateTimer;
    private bool isFacingRight = true;
    private int attackComboCount = 0;  // Penghitung combo attack

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gameInput = new GameInput();
    }

    private void OnEnable()
    {
        gameInput.Player.Enable();
        
        gameInput.Player.Jump.performed += ctx => OnJumpInput();
        gameInput.Player.Dash.performed += ctx => OnDashInput();
        gameInput.Player.Attack.performed += ctx => OnAttackInput();
        gameInput.Player.Stealth.performed += ctx => OnStealthInput();
    }

    private void OnDisable() => gameInput.Player.Disable();

    private void Start()
    {
        currentHealth = maxHealth;
        baseMoveSpeed = moveSpeed;
        currentAttackDuration = baseAttackDuration;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        ChangeState(PlayerState.Idle);
    }

    private void Update()
    {
        // 1. Selalu cek ground di setiap frame
        isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);

        // 2. Berikan data isGrounded dan isRunning ke Animator di setiap frame
        anim.SetBool("isGrounded", isGrounded);
        
        // Membaca input jalan (A/D)
        horizontalInput = gameInput.Player.Move.ReadValue<Vector2>().x;

        anim.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0.1f);

        FlipController();

        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
                Debug.Log("Dash siap digunakan lagi!");
            }
        }

        if (!canStealth)
        {
            stealthCooldownTimer -= Time.deltaTime;
            if (stealthCooldownTimer <= 0)
            {
                canStealth = true;
                Debug.Log("Stealth siap digunakan lagi!");
            }
        }

        // LOGIKA UPDATE BERDASARKAN STATE
        switch (currentState)
        {
            case PlayerState.Idle:
                afkTimer += Time.deltaTime;
                if (afkTimer >= timeToTurnScary)
                {
                    ChangeState(PlayerState.Scary);
                }

                if (Mathf.Abs(horizontalInput) > 0.1f) ChangeState(PlayerState.Move);
                if (!isGrounded && rb.velocity.y < -0.1f) ChangeState(PlayerState.Airborne);
                break;

            case PlayerState.Move:
                if (Mathf.Abs(horizontalInput) < 0.1f) ChangeState(PlayerState.Idle);
                if (!isGrounded) ChangeState(PlayerState.Airborne);
                break;

            case PlayerState.Airborne:
                // Jika sudah menapak tanah kembali, tentukan apakah ke Idle atau Move
                if (isGrounded && Mathf.Abs(rb.velocity.y) < 0.1f)
                {
                    ChangeState(Mathf.Abs(horizontalInput) > 0.1f ? PlayerState.Move : PlayerState.Idle);
                }
                break;

            case PlayerState.Scary:
                // Jika sedang scary lalu pemain mulai menggerakkan karakter
                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    ChangeState(PlayerState.Move);
                }
                break;

            case PlayerState.Attack:
                // Ambil data properti animasi yang sedang aktif berjalan di Layer 0
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

                // Pastikan Animator memang sedang memutar salah satu dari 3 animasi attack kamu
                if (stateInfo.IsName("Player_Attack 1") || stateInfo.IsName("Player_Attack 2") || stateInfo.IsName("Player_Attack 3"))
                {
                    // KALIBRASI VISUAL: Jika sprite sudah selesai diputar 95% (hampir tuntas)
                    if (stateInfo.normalizedTime >= 0.95f)
                    {
                        // Jika pemain melakukan spam klik selama sabetan tadi berjalan, sambung kombo secara rapi!
                        if (isAttackBuffered && attackComboCount < 3)
                        {
                            isAttackBuffered = false;
                            attackComboCount++;
                            Debug.Log($"Kombo Menyambung Rapi ke: #{attackComboCount}");
                            ChangeState(PlayerState.Attack);
                        }
                        else
                        {
                            // Jika tidak ada input spam baru, kembali ke Idle dan nyalakan toleransi jeda waktu kombo
                            comboResetTimer = comboResetDelay; 
                            ChangeState(PlayerState.Idle);
                        }
                    }
                }
                break;

            case PlayerState.Dash:
            case PlayerState.Stagger:
            case PlayerState.Stealth: 
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    // Jika waktu habis, otomatis kembali berdiri tegak (Idle)
                    ChangeState(PlayerState.Idle);
                }
                break;
        }

        if (currentState != PlayerState.Attack && attackComboCount > 0)
        {
            comboResetTimer -= Time.deltaTime;
            if (comboResetTimer <= 0)
            {
                attackComboCount = 0; // Combo baru resmi hangus jika pemain mendiamkan karakter lewat dari 0.5 detik
                Debug.Log("Waktu jeda habis, combo di-reset ke 0.");
            }
        }
            
            // HITUNG MUNDUR DURASI BUFF AKTIF
            if (activeBuff != BuffType.None)
            {
                buffTimer -= Time.deltaTime;
                if (buffTimer <= 0)
                {
                    ResetPlayerStats(); // Kembalikan ke normal jika waktu habis
                }
            }

        if (iFrameTimer > 0)
        {
            iFrameTimer -= Time.deltaTime;
        }

        // Cek sentuhan musuh hanya jika player TIDAK sedang kebal, TIDAK mati, dan TIDAK kebal peci haji (Aegis)
        if (iFrameTimer <= 0 && currentState != PlayerState.Dead && currentState != PlayerState.Dash && currentState != PlayerState.Stealth && !isInvincible)
        {
            // Membuat kotak sensor fiktif setinggi tubuh player (Lebar: 0.6, Tinggi: 1.2)
            // Catatan: Jika pivot karaktermu ada di kaki, naikkan posisi pusat kotak sedikit ke atas (+ 0.6f)
            Vector2 playerCenter = new Vector2(transform.position.x, transform.position.y + 0.6f);
            Collider2D touchingEnemy = Physics2D.OverlapBox(playerCenter, new Vector2(0.6f, 1.2f), 0f, enemyLayer);
            if (touchingEnemy != null)
            {
                TakeDamage(enemyContactDamage); // Player otomatis terluka karena menyenggol musuh!
            }
        }
    }

    private void FixedUpdate()
    {
        // JALANKAN FISIKA BERDASARKAN STATE AKTIF
        switch (currentState)
        {
            case PlayerState.Idle:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;

            case PlayerState.Move:
            case PlayerState.Airborne:
            case PlayerState.Stealth:
                rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
                break;
            case PlayerState.Attack:
                rb.velocity = new Vector2(rb.velocity.x * 0.8f, rb.velocity.y);
                break;

            case PlayerState.Dash:
                float dashDir = isFacingRight ? 1 : -1;
                rb.velocity = new Vector2(dashDir * dashSpeed, 0); // Kunci sumur Y agar melesat lurus
                break;

            case PlayerState.Stagger:
            case PlayerState.Dead:
                // Kurangi/hentikan pergerakan saat mengeksekusi state ini
                rb.velocity = new Vector2(rb.velocity.x * 0.9f, rb.velocity.y);
                break;
        }
    }

    // 2. FUNGSI UNTUK PINDAH STATE (Mengatur transisi awal masuk)
    public void ChangeState(PlayerState newState)
    {
        if (currentState == PlayerState.Dead) return;


        // Pastikan centangan isStagger mati setiap kali karakter keluar dari state Stagger
        if (currentState == PlayerState.Stagger)
        {
            anim.SetBool("isStagger", false);
        }

        // Kembalikan badan karakter menjadi padat/normal jika keluar dari mode Stealth
        if (currentState == PlayerState.Stealth)
        {
            SetSpriteAlpha(1f); 
            canStealth = false;
            stealthCooldownTimer = stealthCooldown;
        }

        // Matikan parameter isScary di Animator setiap kali keluar dari state Scary
        if (currentState == PlayerState.Scary)
        {
            anim.SetBool("isScary", false);
        }

        // Reset kecepatan animasi ke normal setiap ganti state
        anim.speed = 1f; 

        currentState = newState;

        switch (currentState)
        {
            case PlayerState.Idle:
                // Transisi diatur otomatis oleh parameter isRunning & isGrounded di Update
                afkTimer = 0f;
                break;

            case PlayerState.Move:
                // Transisi diatur otomatis oleh parameter isRunning & isGrounded di Update
                afkTimer = 0f;
                break;

            case PlayerState.Airborne:
                anim.Play("Player_AirboneBody");
                break;

            case PlayerState.Attack:
                anim.speed = attackSpeedMultiplier;

                currentAttackDuration = baseAttackDuration / attackSpeedMultiplier; 
                stateTimer = currentAttackDuration; 

                // KUNCI UTAMA: Kirim data angka combo ke parameter Int Animator sebelum memicu trigger!
                anim.SetInteger("comboStep", attackComboCount);

                anim.SetTrigger("Attack");
                ExecuteAttackDamage();

                float nudgeDir = isFacingRight ? 1f : -1f;
                rb.velocity = new Vector2(nudgeDir * 2.8f, rb.velocity.y); 
                break;

            case PlayerState.Dash:
                anim.SetTrigger("Dash"); // Picu trigger Dash di kedua layer
                anim.speed = 2f;         // Percepat pemutaran animasi agar terasa melesat
                stateTimer = dashDuration;
                canDash = false;
                dashCooldownTimer = dashCooldown; // Mulai hitung mundur cooldown
                break;

            case PlayerState.Stagger:
                anim.SetBool("isStagger", true); // Picu animasi terkejut/hit
                stateTimer = staggerDuration;
                break;

            case PlayerState.Stealth:
                // Bisa tambah logic untuk stealth mode di sini
                SetSpriteAlpha(0.5f); 
                stateTimer = stealthDuration;
                afkTimer = 0f;
                break;

            case PlayerState.Dead:
                anim.SetTrigger("Dead");
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
                break;
            case PlayerState.Scary:
                anim.SetBool("isScary", true);
                rb.velocity = Vector2.zero;
                break;
        }
    }

    // GETTER: Untuk akses combo count dari script lain (UI display, etc)
    public int GetComboCount() => attackComboCount;

    public PlayerState GetCurrentState() => currentState;

    public float GetStealthFillAmount()
    {
        // 1. Jika sedang dalam mode ghaib, fill dikuras habis (0)
        if (currentState == PlayerState.Stealth) 
            return 0f;

        // 2. Jika sedang cooldown, hitung persentase pengisian (0 hingga 1)
        if (!canStealth) 
            return 1f - (stealthCooldownTimer / stealthCooldown);

        // 3. Jika siap digunakan, fill penuh (1)
        return 1f;
    }

    private bool CanCancelState(PlayerState stateToCheck, float cancelThreshold)
    {
        if (currentState != stateToCheck) return false;

        // Jika yang dicek adalah Attack, baca progress-nya langsung dari Animator
        if (stateToCheck == PlayerState.Attack)
        {
            return anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= cancelThreshold;
        }

        // Untuk state selain attack (seperti Stagger/Dash), tetap gunakan rumus timer C# asli
        float timePassed = GetStateDuration(stateToCheck) - stateTimer;
        float animationProgress = timePassed / GetStateDuration(stateToCheck);
        
        return animationProgress >= cancelThreshold;
    }

    // Dapatkan durasi state berdasarkan jenis state
    private float GetStateDuration(PlayerState state)
    {
        return state switch
        {
            PlayerState.Attack => currentAttackDuration,
            PlayerState.Dash => dashDuration,
            PlayerState.Stagger => staggerDuration,
            _ => 1f
        };
    }

    // 3. LOGIKA KETIKA TOMBOL INPUT DI TEKAN
    private void OnJumpInput()
    {
        afkTimer = 0f; // Reset waktu AFK

        if (currentState == PlayerState.Scary)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            ChangeState(PlayerState.Airborne);
            anim.Play("Player_AirboneBody"); // Paksa visual langsung ganti ke animasi lompat
            return; // Keluar dari fungsi agar kode di bawah tidak dieksekusi lagi
        }

        if ((currentState == PlayerState.Idle || currentState == PlayerState.Move || currentState == PlayerState.Stealth || currentState == PlayerState.Attack) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            ChangeState(PlayerState.Airborne);
            anim.Play("Player_AirboneBody"); 
        }
    }

    private void OnDashInput()
    {
        if (!canDash) 
        {
            return; // Abaikan input jika dash masih cooldown
        }

        // ANIMATION CANCELLING: Dash bisa membatalkan Attack atau Stagger jika sudah melewati cancel point
        if (currentState == PlayerState.Attack || CanCancelState(PlayerState.Attack, dashCancelThreshold))
        {
            Debug.Log("Dash Cancel Attack!");
            ChangeState(PlayerState.Dash);
            return;
        }
        
        if (CanCancelState(PlayerState.Stagger, staggerCancelThreshold))
        {
            Debug.Log("Dash Cancel Stagger!");
            ChangeState(PlayerState.Dash);
            return;
        }

        // Original logic: Boleh dash dari hampir semua state (kecuali Dash dan Dead)
        if (currentState != PlayerState.Stagger && currentState != PlayerState.Dead && currentState != PlayerState.Dash)
        {
            ChangeState(PlayerState.Dash);
        }
    }

    private void OnAttackInput()
    {
        afkTimer = 0f; // Reset waktu AFK

        // FASE A: Jika pemain mengklik AKTIF saat animasi attack sebelumnya sedang berjalan (Rapid Click/Buffer)
        if (currentState == PlayerState.Attack)
        {
            if (attackComboCount < 3 && !isAttackBuffered)
            {
                isAttackBuffered = true;
                Debug.Log($"Klik cepat dideteksi! Antrean combo berikutnya disimpan.");
            }
            return;
        }

        // FASE B: Jika pemain mengklik saat posisi Idle/Jalan (Bisa Attack baru ATAU kelanjutan dari jeda Idle)
        if (currentState == PlayerState.Idle || currentState == PlayerState.Move || currentState == PlayerState.Stealth || currentState == PlayerState.Airborne || currentState == PlayerState.Scary)
        {
            {
                // Jika timer toleransi jeda masih aktif, naikkan tingkat combo
                if (attackComboCount > 0 && attackComboCount < 3)
                {
                    attackComboCount++;
                    Debug.Log($"Melanjutkan combo setelah jeda! Lanjut ke Langkah #{attackComboCount}");
                }
                else
                {
                    attackComboCount = 1; // Mulai dari awal jika jeda toleransi sudah hangus
                    Debug.Log("Mulai serangan baru dari Awal (Combo #1)");
                }

                isAttackBuffered = false;
                ChangeState(PlayerState.Attack);
            }
        }
    }

    private void OnStealthInput()
    {
        afkTimer = 0f; // Reset waktu AFK

        if (!canStealth && currentState != PlayerState.Stealth)
        {
            Debug.Log($"Stealth masih cooldown! Sisa waktu: {Mathf.CeilToInt(stealthCooldownTimer)} detik.");
            return;
        }

        if (currentState == PlayerState.Idle || currentState == PlayerState.Move)
        {
            ChangeState(PlayerState.Stealth);
            Debug.Log("Masuk ke Stealth");
        }
        else if (currentState == PlayerState.Stealth)
        {
            // Jika ditekan lagi saat mode stealth, kembali ke normal
            ChangeState(PlayerState.Idle);
            Debug.Log("Keluar dari Stealth");
        }
    }

    // Fungsi luar untuk dipanggil oleh script Musuh/Damage ketika player terkena Hit
    public void TakeDamage(int damage)
    {
        // Proteksi: Jika player sudah mati, abaikan damage berikutnya
        if (currentState == PlayerState.Dead || isInvincible || iFrameTimer > 0) return;

        // 1. Kurangi darah player
        currentHealth -= damage;
        UpdateHealthUI();
        Debug.Log($"Player terkena hit! Sisa darah: {currentHealth}");

        // 2. Aktifkan waktu kebal sesaat agar tidak mati instan
        iFrameTimer = iFrameDuration;

        // 3. CEK KONDISI DARAH HABIS ATAU MASIH ADA
        if (currentHealth <= 0)
        {
            currentHealth = 0;

            // KONDISI MATI: Berikan efek dorongan vertikal/horizontal kecil saat ambruk (opsional)
            float dieKnockbackDir = isFacingRight ? -1f : 1f;
            rb.velocity = new Vector2(dieKnockbackDir * 3f, 5f); // Sedikit terlempar lalu jatuh

            ChangeState(PlayerState.Dead); 
        }
        else
        {
            // KONDISI STAGGER (Darah masih ada): Logika Fallback/Knockback
            // Jika karakter menghadap kanan, dorong ke kiri (-1). Jika menghadap kiri, dorong ke kanan (1).
            float fallbackDirection = isFacingRight ? -1f : 1f;
            
            // Atur kekuatan fallback (Contoh: Mundur horizontal = 6f, Melompat sedikit karena efek benturan = 4f)
            rb.velocity = new Vector2(fallbackDirection * 6f, 4f);

            ChangeState(PlayerState.Stagger); 
        }
    }

    // TAMBAHKAN FUNGSI BARU INI DI BAGIAN BAWAH SCRIPT KAMU:
    private void ExecuteAttackDamage()
    {
        // 1. Buat lingkaran fiktif di posisi attackPoint untuk mendeteksi semua objek di layer musuh
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // 2. Lakukan perulangan untuk setiap musuh yang terkena lingkaran sabetan
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"Sabetan mengenai: {enemy.name}");

            // 3. Hubungkan dengan script milik musuh (Misal nama script musuhmu adalah EnemyHealth)
            // Ganti "EnemyHealth" di bawah sesuai dengan nama script darah musuh yang kamu/tim buat nanti
            if (enemy.TryGetComponent<EnemyBase>(out EnemyBase enemyComponent))
            {
                // Kirim damage ke musuh melalui fungsinya
                enemyComponent.TakeDamage(attackDamage);
            }
        }
    }

    public void IncreaseAttackSpeed(float amount)
    {
        attackSpeedMultiplier += amount;
        Debug.Log($"Attack Speed Meningkat! Multiplier saat ini: {attackSpeedMultiplier}");
    }

    private void UpdateHealthUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }
    }

    private void FlipController()
    {
        if (currentState == PlayerState.Dash || currentState == PlayerState.Stagger || currentState == PlayerState.Dead) return;

        if (horizontalInput > 0.1f && !isFacingRight) Flip();
        else if (horizontalInput < -0.1f && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }

        // TAMBAHKAN KODE BARU INI DI SINI:
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange); // Menampilkan lingkaran kuning area serang
        }
    }

    // Dipanggil saat menginjak Es Cekek
    public void ApplyHealthDrop(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
        Debug.Log($"HP bertambah! HP Sekarang: {currentHealth}");
    }

    // Dipanggil saat menginjak salah satu dari 3 item buff
    public void ApplyBuff(BuffType newBuff, float duration)
    {
        // 1. Bersihkan dulu efek status buff lama agar tidak menumpuk permanen
        ResetPlayerStats();

        // 2. Pasang identitas buff baru dan setel ulang timernya
        activeBuff = newBuff;
        buffTimer = duration;

        // 3. Terapkan efek parameter sesuai gambar template
        switch (activeBuff)
        {
            case BuffType.Haste:
                moveSpeed = baseMoveSpeed * 1.5f; // Sandal swallow: Speed jalan x 1.5
                Debug.Log("Buff Haste Aktif: Gerakan lebih lincah!");
                break;

            case BuffType.Berserk:
                attackSpeedMultiplier = 2f; // Kopi bapak: Animasi serang dipercepat 30%
                Debug.Log("Buff Berserk Aktif: Sabetan sarung lebih cepat!");
                break;

            case BuffType.Aegis:
                isInvincible = true; // Peci haji: Kebal dari segala jenis damage
                Debug.Log("Buff Aegis Aktif: Karakter kebal damage!");
                break;
        }
    }

    // Fungsi pembantu untuk mengembalikan tubuh player ke kondisi normal semula
    public void ResetPlayerStats()
    {
        activeBuff = BuffType.None;
        moveSpeed = baseMoveSpeed; // Kembalikan speed jalan ke normal
        attackSpeedMultiplier = 1f; // Kembalikan kecepatan sabetan ke normal
        isInvincible = false;      // Matikan mode kebal
        iFrameTimer = 0f;          // Reset timer kebal
        Debug.Log("Durasi buff habis, status player kembali normal.");
    }

    // Fungsi pembantu untuk mengubah nilai transparansi (Alpha) karakter
    private void SetSpriteAlpha(float alpha)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha; // Mengubah nilai Alpha (transparansi)
            spriteRenderer.color = color;
        }
    }
}