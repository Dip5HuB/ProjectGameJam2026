using UnityEngine;

public class EnemyGenderuwo : EnemyBase
{
    [Header("Genderuwo Zoning Settings")]
    [SerializeField] private float attackRangeThreshold = 2.2f; // Jarak dekat untuk mulai memukul

    [Header("Hulk Smash Settings")]
    [SerializeField] private float telegraphDuration = 0.7f;   // Waktu mengangkat tangan ke atas
    [SerializeField] private float stuckDuration = 1.2f;       // Waktu tangan tersangkut di lantai (Recovery)
    [SerializeField] private int slamDamage = 35;              // Damage hantaman besar
    [SerializeField] private float slamRadius = 1.8f;          // Radius lingkaran ledakan AoE
    [SerializeField] private Transform slamPoint;              // Titik pusat hantaman di tanah

    // Tracker Fase Serangan Internal
    private enum AttackPhase { Telegraph, Execution, Stuck }
    private AttackPhase currentPhase;
    private float phaseTimer;
    private bool damageApplied = false;

    protected override void UpdateIdleState()
    {
        // Kondisi Awal: Menggantung diam seperti jas hujan besar di paku tembok.
        // Jika player masuk area pandang, Genderuwo turun/aktif mengejar.
        if (IsPlayerInAggroRange())
        {
            if (anim != null) anim.SetBool("isActive", true);
            ChangeState(EnemyState.Move);
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        // 1. PERGERAKAN LAMBAT (Heavy Stomping): Hitung arah horizontal (X) menuju player
        float directionX = playerTransform.position.x > transform.position.x ? 1f : -1f;
        
        // Genderuwo menyusuri tanah dengan kecepatan lambat (moveSpeed bawaan diatur kecil saja di Inspector, misal: 1.5)
        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);

        // 2. Mengatur arah hadap visual monster
        FlipSprite(directionX);

        // 3. CEK JARAK SERANG
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRangeThreshold)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y); // Berhenti melangkah total
            ChangeState(EnemyState.Attack);
            
            // Masuk ke Fase 1: Telegraph (Mengangkat tangan ke atas)
            currentPhase = AttackPhase.Telegraph;
            phaseTimer = telegraphDuration;
            damageApplied = false;

            if (anim != null) anim.SetTrigger("LiftArms");
        }
    }

    protected override void UpdateAttackState()
    {
        phaseTimer -= Time.deltaTime;

        switch (currentPhase)
        {
            // ==========================================
            // FASE 1: ANGGKAT TANGAN (Telegraph)
            // ==========================================
            case AttackPhase.Telegraph:
                rb.velocity = new Vector2(0f, rb.velocity.y); // Kunci posisi agar tidak bergeser

                if (phaseTimer <= 0)
                {
                    // Waktu angkat tangan habis, hantam ke tanah!
                    currentPhase = AttackPhase.Execution;
                    if (anim != null) anim.SetTrigger("SlamEarth");
                }
                break;

            // ==========================================
            // FASE 2: HANTAMAN TANAH (Execution AoE)
            // ==========================================
            case AttackPhase.Execution:
                if (!damageApplied)
                {
                    damageApplied = true;
                    ExecuteAoESlam(); // Picu ledakan getaran damage di tanah
                    
                    // Langsung pindah ke Fase 3 di frame yang sama (Tangan tersangkut)
                    currentPhase = AttackPhase.Stuck;
                    phaseTimer = stuckDuration;
                }
                break;

            // ==========================================
            // FASE 3: TANGAN TERSANGKUT (Stuck / Recovery)
            // ==========================================
            case AttackPhase.Stuck:
                rb.velocity = new Vector2(0f, rb.velocity.y); // Genderuwo terkunci total tidak bisa jalan

                if (phaseTimer <= 0)
                {
                    // Tangan berhasil dicabut dari tanah, kembali berjalan lambat mengejar player
                    if (anim != null) anim.SetTrigger("RecoverArms");
                    ChangeState(EnemyState.Move);
                }
                break;
        }
    }

    private void ExecuteAoESlam()
    {
        Debug.Log($"{gameObject.name} Menghantam Tanah! Booom!");

        // Deteksi apakah tubuh player berada di dalam radius lingkaran ledakan AoE milik Genderuwo
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(slamPoint.position, slamRadius, playerLayer);
        
        foreach (Collider2D obj in hitObjects)
        {
            // Kirim damage ke skrip PlayerController yang kita buat sebelumnya
            if (obj.TryGetComponent<PlayerController>(out PlayerController player))
            {
                player.TakeDamage(slamDamage);
            }
        }
    }

    // Fungsi pembalik arah hadap otomatis (Flip)
    private void FlipSprite(float horizontalDir)
    {
        if (horizontalDir > 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontalDir < 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    // Menggambar lingkaran fiktif di Scene editor agar mudah mengatur luas jangkauan pukulannya
    private void OnDrawGizmosSelected()
    {
        if (slamPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(slamPoint.position, slamRadius);
        }
    }
}