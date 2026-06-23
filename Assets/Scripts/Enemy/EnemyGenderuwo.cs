using UnityEngine;

public class EnemyGenderuwo : EnemyBase
{
    [Header("Genderuwo Stomp Settings")]
    [SerializeField] private float slamRadius = 2f;
    [SerializeField] private int slamDamage = 40;
    [SerializeField] private Transform slamPoint;
    [SerializeField] private float armStuckDuration = 1.2f; // Waktu tangan tersangkut di tanah

    private bool attackExecuted = false;

    protected override void UpdateIdleState()
    {
        // Menggantung diam di paku tembok, jika didekati langsung aktif
        if (IsPlayerInAggroRange())
        {
            ChangeState(EnemyState.Move);
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        // Berjalan sangat lambat (Heavy Stomping) hanya di sumbu X (menyusuri lantai)
        float direction = playerTransform.position.x > transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        // Putar arah hadap
        transform.localScale = new Vector3(direction > 0 ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // Jika player sangat dekat, angkat tangan untuk memukul (Hulk Slam)
        if (Vector2.Distance(transform.position, playerTransform.position) <= 1.8f)
        {
            ChangeState(EnemyState.Attack);
            stateTimer = 0.7f; // Durasi mengangkat tangan ke atas (Telegraph)
            attackExecuted = false;
            rb.velocity = Vector2.zero;
            if (anim != null) anim.SetTrigger("LiftArms");
        }
    }

    protected override void UpdateAttackState()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0 && !attackExecuted)
        {
            // EKSEKUSI: Menghempaskan tangan ke tanah (AoE Slam)
            attackExecuted = true;
            stateTimer = armStuckDuration; // Masuk ke fase recovery (tangan tersangkut)
            
            if (anim != null) anim.SetTrigger("Slam");

            // Sensor area getaran gempa di tanah
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(slamPoint.position, slamRadius, playerLayer);
            foreach (Collider2D player in hitPlayers)
            {
                if (player.TryGetComponent<PlayerController>(out PlayerController pc))
                {
                    pc.TakeDamage(slamDamage);
                }
            }
        }
        else if (attackExecuted && stateTimer <= 0)
        {
            // Tangan berhasil dicabut dari tanah, kembali mengejar player
            ChangeState(EnemyState.Move);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (slamPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(slamPoint.position, slamRadius);
        }
    }
}