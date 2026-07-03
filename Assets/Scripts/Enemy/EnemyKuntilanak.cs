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
        
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    public override void ChangeState(EnemyState newState)
    {
        base.ChangeState(newState); // Tetap jalankan fungsi internal milik EnemyBase

        if (anim == null) return;

        // Jika pindah ke Idle, matikan animasi (kembali ke state Idle berkecepatan 0)
        if (newState == EnemyState.Idle)
        {
            anim.SetBool("isFloating", false);
        }
        // Jika pindah ke Move, hidupkan animasi melayang jalan
        else if (newState == EnemyState.Move)
        {
            anim.SetBool("isFloating", true);
        }
    }

    protected override void UpdateIdleState()
    {
        // Diam kaku di tempat sampai player masuk radius deteksi (aggroRange)
        if (IsPlayerInAggroRange())
        {
            ChangeState(EnemyState.Move); // Otomatis memicu anim.SetBool("isFloating", true) di atas
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        if (!IsPlayerInAggroRange())
        {
            rb.velocity = Vector2.zero;
            ChangeState(EnemyState.Idle); // Kembali diam kaku
            return;
        }

        // 1. GERAKAN MELAYANG MENGEJAR PLAYER
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        // 2. Atur arah hadap visual sprite
        FlipSprite(direction.x);

        // 3. CEK JARAK SERANG
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRangeThreshold)
        {
            rb.velocity = Vector2.zero; 
            ChangeState(EnemyState.Attack);
            
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

            case AttackPhase.Dashing:
                rb.velocity = lockedDashDirection * moveSpeed * dashSpeedMultiplier;
                break;

            case AttackPhase.Recover:
                rb.velocity = Vector2.zero; 

                if (phaseTimer <= 0)
                {
                    // Setelah menyerang selesai, cek kembali apakah player masih di dalam radius deteksi
                    ChangeState(IsPlayerInAggroRange() ? EnemyState.Move : EnemyState.Idle);
                }
                break;
        }

        if (currentPhase == AttackPhase.Dashing && phaseTimer <= 0)
        {
            currentPhase = AttackPhase.Recover;
            phaseTimer = recoverDuration;
            rb.velocity = Vector2.zero;
        }
    }

    private void FlipSprite(float horizontalDir)
    {
        if (horizontalDir > 0.1f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontalDir < -0.1f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}