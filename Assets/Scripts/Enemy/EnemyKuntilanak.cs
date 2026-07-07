using UnityEngine;

public class EnemyKuntilanak : EnemyBase
{
    [Header("Kuntilanak Floating Settings")]
    [SerializeField] private float attackRangeThreshold = 3f; // Jarak ideal untuk mulai menyerang

    [Header("Dash Attack Settings")]
    [SerializeField] private float telegraphDuration = 0.6f;    // Waktu ancang-ancang (berhembus)
    [SerializeField] private float dashDuration = 0.4f;         // Durasi menerjang lurus
    [SerializeField] private float dashSpeedMultiplier = 3.5f;  // Kecepatan laju terjangan
    [SerializeField] private float recoverDuration = 0.5f;       // Jeda sebelum mengejar lagi

    // Tracker Fase Serangan Internal
    private enum AttackPhase { Telegraph, Dashing, Recover }
    private AttackPhase currentPhase;
    private float phaseTimer;
    private Vector2 lockedDashDirection; // Mengunci koordinat arah agar tidak magnetis/ketarik

    protected override void Start()
    {
        base.Start();
        
        // KUNCI UTAMA: Matikan gravitasi total karena Kuntilanak bergerak melayang di udara
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    protected override void UpdateIdleState()
    {
        // Kondisi awal: Menggantung kaku di tali jemuran
        // Jika player masuk radius deteksi, aktifkan mode melayang mengejar (Move)
        if (IsPlayerInAggroRange())
        {
            if (anim != null) anim.SetBool("isFloating", true);
            ChangeState(EnemyState.Move);
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        // 1. GERAKAN MELAYANG GARIS LURUS: Hitung vektor arah menuju koordinat Player (X dan Y)
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        
        // Gerakkan rigidbody secara halus ke arah posisi pemain
        rb.velocity = direction * moveSpeed;

        // 2. Atur arah hadap visual sprite berdasarkan posisi sumbu X pemain
        FlipSprite(direction.x);

        // 3. CEK JARAK SERANG: Jika jarak garis lurus sudah dekat, langsung kunci posisi untuk menyerang
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRangeThreshold)
        {
            rb.velocity = Vector2.zero; // Berhenti melayang sejenak
            ChangeState(EnemyState.Attack);
            
            // Masuk ke Fase 1: Telegraph (Ancang-ancang tiupan angin)
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
                rb.velocity = Vector2.zero; // Kunci total di udara saat ambil ancang-ancang

                if (phaseTimer <= 0)
                {
                    currentPhase = AttackPhase.Dashing;
                    phaseTimer = dashDuration;

                    // SOLUSI BUG KETARIK: Kunci arah garis lurus ke target di frame ini saja!
                    // Selama menerjang berjalan, pergerakan menghindar dari Player akan diabaikan total.
                    lockedDashDirection = (playerTransform.position - transform.position).normalized;

                    if (anim != null) anim.SetTrigger("DashAttack");
                }
                break;

            // ==========================================
            // FASE 2: MENERJANG LURUS (Dashing)
            // ==========================================
            case AttackPhase.Dashing:
                // Melesat lurus menembus udara memanfaatkan vektor arah yang sudah dikunci
                rb.velocity = lockedDashDirection * moveSpeed * dashSpeedMultiplier;
                break;

            // ==========================================
            // FASE 3: PEMULIHAN / DIAM SESAAT (Recover)
            // ==========================================
            case AttackPhase.Recover:
                rb.velocity = Vector2.zero; // Rem instan di udara setelah durasi dash selesai

                if (phaseTimer <= 0)
                {
                    // Selesai menyerang, kembali ke mode melayang mengejar pemain
                    ChangeState(EnemyState.Move);
                }
                break;
        }

        // Cek perpindahan fase durasi dash ke recover secara mandiri
        if (currentPhase == AttackPhase.Dashing && phaseTimer <= 0)
        {
            currentPhase = AttackPhase.Recover;
            phaseTimer = recoverDuration;
            rb.velocity = Vector2.zero;
        }
    }

    // Fungsi pembalik arah hadap otomatis (Flip)
    private void FlipSprite(float horizontalDir)
    {
        if (horizontalDir > 0.1f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontalDir < -0.1f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}