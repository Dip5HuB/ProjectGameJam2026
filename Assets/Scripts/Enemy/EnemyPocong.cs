using UnityEngine;

public class EnemyPocong : EnemyBase
{
    [Header("Pocong Hopping Mechanics")]
    [SerializeField] private float jumpForceX = 4f;
    [SerializeField] private float jumpForceY = 5f;
    [SerializeField] private float hopDelay = 1.2f; // Jeda antar lompatan
    private float hopTimer;
    private bool isHopping = false;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("New Dash & Rotate Mechanics")]
    [SerializeField] private float attackRangeThreshold = 2.5f; // Jarak pemicu serangan
    [SerializeField] private float telegraphDuration = 0.5f;    // Durasi ancang-ancang
    [SerializeField] private float dashDuration = 0.35f;         // Durasi meluncur/dash
    [SerializeField] private float dashSpeedMultiplier = 4f;     // Kekuatan laju dash
    [SerializeField] private float recoverDuration = 0.4f;       // Waktu bangun kembali
    private float originalGravity;

    // Tracker Fase Serangan Internal
    private enum AttackPhase { Telegraph, Dashing, Recover }
    private AttackPhase currentPhase;
    private float phaseTimer;
    private float lockedDashDirection; // Mengunci arah agar tidak terjadi bug ketarik/magnet

    protected override void Start()
    {
        base.Start(); // Tetap jalankan fungsi Start milik EnemyBase
        
        // Simpan nilai gavitasi asli yang kamu setel di Inspector (misal: 1 atau 2)
        if (rb != null) originalGravity = rb.gravityScale; 
    }
    
    protected override void UpdateIdleState()
    {
        // Diam tegak lurus, pindah ke Move jika player masuk area aggro
        if (IsPlayerInAggroRange())
        {
            ChangeState(EnemyState.Move);
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        // 1. Hitung arah ke player untuk keperluan visual hadap & lompat
        float direction = playerTransform.position.x > transform.position.x ? 1f : -1f;
        FlipSprite(direction);

        // 2. Hitung waktu mundur jeda lompat & cek tanah
        hopTimer -= Time.deltaTime;
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // LOGIKA LOMPAT RITMIS
        if (isGrounded && !isHopping && hopTimer <= 0)
        {
            isHopping = true;
            rb.velocity = new Vector2(direction * jumpForceX, jumpForceY);
            if (anim != null) anim.SetTrigger("Jump");
        }

        // MENDARAT: Berikan rem horizontal agar tidak meluncur licin
        if (isGrounded && isHopping && rb.velocity.y <= 0.1f)
        {
            isHopping = false;
            hopTimer = hopDelay;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        // 3. CEK JARAK SERANG: Jika sudah pas, masuk ke fase menyerang
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRangeThreshold)
        {
            rb.velocity = Vector2.zero; // Rem total posisi sebelum ancang-ancang
            ChangeState(EnemyState.Attack);
            
            // Mulai dari Fase 1: Telegraph (Ancang-Ancang)
            currentPhase = AttackPhase.Telegraph;
            phaseTimer = telegraphDuration;
        }
    }

    protected override void UpdateAttackState()
    {
        phaseTimer -= Time.deltaTime;

        switch (currentPhase)
        {
            // ==========================================
            // FASE 1: ANCANG-ANCANG (Telegraph)
            // ==========================================
            case AttackPhase.Telegraph:
                rb.velocity = new Vector2(0, rb.velocity.y); // Kunci posisi X agar diam tegak

                if (phaseTimer <= 0)
                {
                    // Transisi ke Fase 2 (Dashing)
                    currentPhase = AttackPhase.Dashing;
                    phaseTimer = dashDuration;

                    lockedDashDirection = playerTransform.position.x > transform.position.x ? 1f : -1f;

                    // MATIKAN GRAVITASI SEBELUM MELUNCUR
                    rb.gravityScale = 0f; 
                    rb.velocity = new Vector2(rb.velocity.x, 0f);

                    // ROTASI 90 DERAJAT: Rebah tidur horizontal sesuai arah terjangnya
                    float zRotation = lockedDashDirection > 0 ? -90f : 90f;
                    transform.localEulerAngles = new Vector3(0, 0, zRotation);
                    
                    if (anim != null) anim.SetTrigger("DashAttack");
                }
                break;

            // ==========================================
            // FASE 2: MELUNCUR (Dashing)
            // ==========================================
            case AttackPhase.Dashing:
                // Dorong lurus secara fisik murni menggunakan arah yang sudah dikunci
                rb.velocity = new Vector2(lockedDashDirection * moveSpeed * dashSpeedMultiplier, 0f);

                if (phaseTimer <= 0)
                {
                    // Transisi ke Fase 3 (Recover / Bangun)
                    currentPhase = AttackPhase.Recover;
                    phaseTimer = recoverDuration;

                    rb.gravityScale = originalGravity; // Kembalikan gravitasi ke nilai asli

                    // KEMBALI TEGAK LURUS (0 Derajat)
                    transform.localEulerAngles = Vector3.zero;
                    rb.velocity = new Vector2(0, rb.velocity.y); // Rem setelah melewati player
                }
                break;

            // ==========================================
            // FASE 3: BANGUN & MODAL BALIK BADAN (Recover)
            // ==========================================
            case AttackPhase.Recover:
                rb.velocity = new Vector2(0, rb.velocity.y); // Diam sejenak proses berdiri

                if (playerTransform != null)
                {
                    // LANGSUNG BERBALIK ARAH menghadap player kembali saat berdiri
                    float faceDir = playerTransform.position.x > transform.position.x ? 1f : -1f;
                    FlipSprite(faceDir);
                }

                if (phaseTimer <= 0)
                {
                    // Selesai seluruh rangkaian serangan, kembali melompat normal mengejar player
                    ChangeState(EnemyState.Move);
                    hopTimer = hopDelay; // Berikan jeda nafas sebelum lompat lagi
                }
                break;
        }
    }

    // Fungsi pembantu universal untuk mengatur visual arah hadap musuh
    private void FlipSprite(float dir)
    {
        if (dir > 0)
        {
            // Player di sebelah KANAN -> Pocong harus hadap KANAN (Scale X harus POSITIF)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (dir < 0)
        {
            // Player di sebelah KIRI -> Pocong harus hadap KIRI (Scale X harus NEGATIF)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}