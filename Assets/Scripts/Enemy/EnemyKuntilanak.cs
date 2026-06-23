using UnityEngine;

public class EnemyKuntilanak : EnemyBase
{
    [Header("Kuntilanak Fly Settings")]
    [SerializeField] private float flySmoothness = 2f;
    [SerializeField] private float attackDashSpeed = 12f;
    [SerializeField] private float attackRangeThreshold = 2.5f;

    private bool isDashing = false;

    protected override void Start()
    {
        base.Start();
        // Kunci gravitasi ke 0 karena Kuntilanak adalah hantu melayang (Flying Enemy)
        rb.gravityScale = 0f; 
    }

    protected override void UpdateIdleState()
    {
        // Menggantung kaku di tali jemuran, tunggu player masuk jarak aggro
        if (IsPlayerInAggroRange())
        {
            if (anim != null) anim.SetBool("isFloating", true);
            ChangeState(EnemyState.Move);
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        // Melayang perlahan mendekati pemain secara garis lurus (X dan Y diikuti)
        Vector2 targetPosition = playerTransform.position;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Putar arah hadap sprite berdasarkan posisi player
        if (playerTransform.position.x > transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // Jika sudah dekat, masuk ke fase Attack (Telegraph)
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRangeThreshold)
            {
            ChangeState(EnemyState.Attack);
            stateTimer = 0.6f; // Waktu ancang-ancang berhembus tiupan angin angin kencang
            isDashing = false;
            rb.velocity = Vector2.zero;
        }
    }

    protected override void UpdateAttackState()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0 && !isDashing)
        {
            isDashing = true;
            stateTimer = 0.4f; // Durasi meluncur kencang ke pemain

            // Berhembus lurus menerjang target lokasi terakhir player
            Vector2 dashDirection = (playerTransform.position - transform.position).normalized;
            rb.velocity = dashDirection * attackDashSpeed;
        }
        else if (isDashing && stateTimer <= 0)
        {
            // Selesai menerjang, kembali melayang normal
            rb.velocity = Vector2.zero;
            ChangeState(EnemyState.Move);
        }
    }
}