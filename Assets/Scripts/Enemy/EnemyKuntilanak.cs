using UnityEngine;

public class EnemyKuntilanak : EnemyBase
{
    [Header("Kuntilanak Floating Settings")]
    [SerializeField] private float attackRangeThreshold = 3f; 

    [Header("Dash Attack Settings")]
    [SerializeField] private float telegraphDuration = 0.6f;    
    [SerializeField] private float dashDuration = 0.4f;         
    [SerializeField] private float dashSpeedMultiplier = 3.5f;  
    [SerializeField] private float recoverDuration = 0.5f;       

    private enum AttackPhase { Telegraph, Dashing, Recover }
    private AttackPhase currentPhase;
    private float phaseTimer;
    private Vector2 lockedDashDirection; 

    protected override void Start()
    {
        base.Start();
        
        // Kuntilanak hantu melayang, matikan pengaruh berat gravitasi bumi
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }
    
    // OVRERIDE SYSTEM: Memaksa Animator responsif tanpa delay frame
    public override void ChangeState(EnemyState newState)
    {
        base.ChangeState(newState); // Jalankan fungsi dasar EnemyBase

        if (anim == null) return;

        switch (newState)
        {
            case EnemyState.Idle:
                anim.SetBool("isFloating", false);
                anim.Play("Kunti_Idle"); // Paksa visual instan masuk frame diam
                break;

            case EnemyState.Move:
                anim.SetBool("isFloating", true);
                anim.Play("Kunti_move"); // Paksa visual instan masuk frame melayang
                break;

            case EnemyState.Stagger:
                anim.Play("Kunti_Stagger"); // Potong paksa visual jika mendadak kena tebas sarung
                break;

            case EnemyState.Dead:
                anim.Play("Kunti_Dead"); // Potong paksa visual ke kondisi mati
                break;
        }
    }

    protected override void UpdateIdleState()
    {
        if (IsPlayerInAggroRange())
        {
            ChangeState(EnemyState.Move); 
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        if (!IsPlayerInAggroRange())
        {
            rb.velocity = Vector2.zero;
            ChangeState(EnemyState.Idle); 
            return;
        }

        // 1. GERAKAN MELAYANG MENGEJAR PLAYER (Mendukung sumbu X dan Y secara halus)
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        // 2. Atur arah hadap visual sprite
        FlipSprite(direction.x);

        // 3. CEK JARAK SERANG
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRangeThreshold)
        {
            rb.velocity = Vector2.zero; 
            ChangeState(EnemyState.Attack);
            
            // Setel parameter awal serangan
            currentPhase = AttackPhase.Telegraph;
            phaseTimer = telegraphDuration;
            lockedDashDirection = (playerTransform.position - transform.position).normalized;
            
            FlipSprite(lockedDashDirection.x);
        }
    }

    protected override void UpdateAttackState()
    {
        phaseTimer -= Time.deltaTime;

        switch (currentPhase)
        {
            // FASE 1: ANCANG-ANCANG (Telegraph)
            case AttackPhase.Telegraph:
                rb.velocity = Vector2.zero; 

                if (phaseTimer <= 0)
                {
                    currentPhase = AttackPhase.Dashing;
                    phaseTimer = dashDuration;
                    FlipSprite(lockedDashDirection.x);

                    if (anim != null) anim.SetTrigger("DashAttack");
                }
                break;

            // FASE 2: MELUNCUR TERJANG (Dashing)
            case AttackPhase.Dashing:
                rb.velocity = lockedDashDirection * moveSpeed * dashSpeedMultiplier;

                // Transisi otomatis ke Fase 3 jika bensin durasi dash habis
                if (phaseTimer <= 0)
                {
                    currentPhase = AttackPhase.Recover;
                    phaseTimer = recoverDuration;
                    rb.velocity = Vector2.zero; // Rem instan setelah menerjang
                }
                break;

            // FASE 3: JEDA PEMULIHAN (Recover)
            case AttackPhase.Recover:
                rb.velocity = Vector2.zero; 

                if (phaseTimer <= 0)
                {
                    // Evaluasi ulang: Jika player masih di dalam area radar, kejar lagi. Jika kabur, kembali idle.
                    ChangeState(IsPlayerInAggroRange() ? EnemyState.Move : EnemyState.Idle);
                }
                break;
        }
    }

    private void FlipSprite(float horizontalDir)
    {
        // Kondisi jika gambar aset dasar Kuntilanak menghadap ke KIRI
        if (horizontalDir > 0.1f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontalDir < -0.1f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}