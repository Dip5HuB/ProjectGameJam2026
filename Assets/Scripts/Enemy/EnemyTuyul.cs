using UnityEngine;

public class EnemyTuyul : EnemyBase
{
    [Header("Tuyul Erratic Movement")]
    [SerializeField] private float preferredRange = 3.5f;     // Jarak psikologis yang dijaga Tuyul untuk mengecoh
    [SerializeField] private float zigZagFrequency = 8f;     // Kecepatan keliaran belokan zig-zag
    [SerializeField] private float zigZagMagnitude = 4f;     // Lebar jarak belokan zig-zag
    [SerializeField] private float attackCheckCooldown = 1.5f; // Jeda waktu antar keputusan untuk menerkam
    private float attackDecisionTimer;

    [Header("Unpredictable Pounce Settings")]
    [SerializeField] private float telegraphDuration = 0.4f;   // Ancang-ancang singkat ala atlet sprint
    [SerializeField] private float pounceDuration = 0.4f;      // Durasi melayang di udara saat menerkam
    [SerializeField] private float basePounceForceX = 8f;      // Kekuatan dorong horizontal dasar
    [SerializeField] private float minPounceForceY = 4f;       // Batas minimal lompatan vertikal
    [SerializeField] private float maxPounceForceY = 8f;       // Batas maksimal lompatan vertikal (diacak)

    // Tracker Fase Serangan Internal
    private enum AttackPhase { Telegraph, Pouncing, Recover }
    private AttackPhase currentPhase;
    private float phaseTimer;
    private float lockedDirection;
    private float randomizedJumpForceY;

    protected override void Start()
    {
        base.Start();
        attackDecisionTimer = attackCheckCooldown;
    }

    protected override void UpdateIdleState()
    {
        // Kondisi Awal: Menggelung diam di sudut gelap, hanya memperlihatkan dua mata bersinar.
        // Pindah ke mode Move (Mengejar/Mengecoh) jika Gojo mendekat.
        if (IsPlayerInAggroRange())
        {
            if (anim != null) anim.SetBool("isScrambling", true);
            ChangeState(EnemyState.Move);
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float directionX = playerTransform.position.x > transform.position.x ? 1f : -1f;

        // 1. FORMULA ZIG-ZAG (Meliuk Liar): Menggunakan gelombang Sinus pada kecepatan sumbu X
        float zigZagOffset = Mathf.Sin(Time.time * zigZagFrequency) * zigZagMagnitude;

        // 2. LOGIKA MENJAGA JARAK (Mengecoh Pemain)
        if (distanceToPlayer < preferredRange - 0.8f)
        {
            // Jika Player terlalu dekat, Tuyul panik dan merangkak mundur dengan sangat cepat
            rb.velocity = new Vector2((-directionX * moveSpeed * 1.4f) + zigZagOffset, rb.velocity.y);
        }
        else
        {
            // Jika jarak aman, dia merangkak maju meliuk-liuk acak mendekati posisi ideal
            rb.velocity = new Vector2((directionX * moveSpeed) + zigZagOffset, rb.velocity.y);
        }

        // 3. Mengatur arah hadap visual berdasarkan arah jalannya fisika Rigidbody
        FlipSprite(rb.velocity.x);

        // 4. KEPUTUSAN MENYERANG SECARA ACAK (Arah Sulit Ditebak)
        attackDecisionTimer -= Time.deltaTime;
        if (attackDecisionTimer <= 0 && distanceToPlayer <= preferredRange + 1f)
        {
            attackDecisionTimer = attackCheckCooldown; // Reset timer keputusan

            // Peluang 60% untuk mengeksekusi serangan dadakan saat durasi cooldown habis
            if (Random.value < 0.6f)
            {
                rb.velocity = Vector2.zero; // Rem instan di tempat
                ChangeState(EnemyState.Attack);
                
                // Masuk Fase 1: Telegraph (Posisi atlet siap start)
                currentPhase = AttackPhase.Telegraph;
                phaseTimer = telegraphDuration;
                if (anim != null) anim.SetTrigger("TelegraphSprint");
            }
        }
    }

    protected override void UpdateAttackState()
    {
        phaseTimer -= Time.deltaTime;

        switch (currentPhase)
        {
            // ==========================================
            // FASE 1: ANNCANG-ANCANG ATLET (Telegraph)
            // ==========================================
            case AttackPhase.Telegraph:
                rb.velocity = new Vector2(0f, rb.velocity.y); // Kunci posisi diam di lantai

                if (phaseTimer <= 0)
                {
                    currentPhase = AttackPhase.Pouncing;
                    phaseTimer = pounceDuration;

                    // Kunci arah target terkaman
                    lockedDirection = playerTransform.position.x > transform.position.x ? 1f : -1f;
                    FlipSprite(lockedDirection);

                    // TRIK TIDAK BISA DITEBAK: Acak tinggi lompatan vertikalnya secara drastis!
                    // Kadang Tuyul menerkam rendah lurus, kadang melompat melambung tinggi ke atas (Parabola)
                    randomizedJumpForceY = Random.Range(minPounceForceY, maxPounceForceY);

                    // Tembakkan fisik seperti terkaman kucing garong
                    rb.velocity = new Vector2(lockedDirection * basePounceForceX, randomizedJumpForceY);
                    
                    if (anim != null) anim.SetTrigger("Pounce");
                }
                break;

            // ==========================================
            // FASE 2: TERKAMAN PARABOLA (Pouncing / In Air)
            // ==========================================
            case AttackPhase.Pouncing:
                // Biarkan fisika Unity mengontrol lintasan parabolanya di udara
                if (phaseTimer <= 0)
                {
                    currentPhase = AttackPhase.Recover;
                    phaseTimer = 0.3f; // Jeda pemulihan mendarat singkat
                    rb.velocity = new Vector2(0f, rb.velocity.y); // Rem gesekan horizontal saat mendarat
                }
                break;

            // ==========================================
            // FASE 3: MENDARAT & SIAP KELUAR (Recover)
            // ==========================================
            case AttackPhase.Recover:
                rb.velocity = new Vector2(0f, rb.velocity.y);

                if (phaseTimer <= 0)
                {
                    // Kembali merangkak acak mengecoh pemain
                    ChangeState(EnemyState.Move);
                }
                break;
        }
    }

    // Override fungsi mati untuk disesuaikan dengan efek kepulan asap & mengeong
    protected override void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        rb.velocity = Vector2.zero;

        Debug.Log("Tuyul Kalah: *Poof* (Kepulan asap kecil) + *Mengeong sangat kecil*");
        
        if (anim != null) anim.SetTrigger("PoofDeath");

        // Memanggil sistem drop item modular kita
        if (dropSystem != null) dropSystem.DropRandomItem(transform.position);

        Destroy(gameObject, 0.4f); // Hancurkan objek setelah efek asap selesai
    }

    private void FlipSprite(float horizontalVelocity)
    {
        if (horizontalVelocity > 0.2f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontalVelocity < -0.2f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}