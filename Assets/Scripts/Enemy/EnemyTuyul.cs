using UnityEngine;

public class EnemyTuyul : EnemyBase
{
    [Header("Tuyul Trick Settings")]
    [SerializeField] private float scrambleFrequency = 5f; // Kecepatan gelombang zig-zag
    [SerializeField] private float scrambleMagnitude = 3f; // Lebar belokan zig-zag
    [SerializeField] private float safeDistance = 3.5f;     // Jarak tipu daya untuk mundur

    private float zigZagSign = 1f;
    private float behaviorChangeTimer;

    protected override void UpdateIdleState()
    {
        // Menggelung diam memperlihatkan mata bersinar dalam gelap
        if (IsPlayerInAggroRange())
        {
            ChangeState(EnemyState.Move);
            behaviorChangeTimer = 1f;
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float directionX = playerTransform.position.x > transform.position.x ? 1f : -1f;

        behaviorChangeTimer -= Time.deltaTime;
        if (behaviorChangeTimer <= 0)
        {
            // Mengacak pola belokan zig-zag secara ritmis
            zigZagSign = Random.value > 0.5f ? 1f : -1f;
            behaviorChangeTimer = Random.Range(0.4f, 0.8f);
        }

        // LOGIKA MENJAGA JARAK (Mengecoh)
        if (distanceToPlayer < safeDistance)
        {
            // Jika player terlalu dekat, Tuyul merangkak mundur lincah ke belakang
            rb.velocity = new Vector2(-directionX * moveSpeed * 1.2f, rb.velocity.y);
        }
        else
        {
            // Gerakan merangkak acak mendekati (ditambah gaya kosinus untuk efek zig-zag meliuk)
            float zigZagY = Mathf.Cos(Time.time * scrambleFrequency) * scrambleMagnitude;
            rb.velocity = new Vector2(directionX * moveSpeed + (zigZagSign * 2f), rb.velocity.y);
        }

        // Putar arah hadap berdasarkan arah gerak fisika Rigidbody-nya
        transform.localScale = new Vector3(rb.velocity.x > 0 ? -Mathf.Abs(transform.localScale.x) : Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // Peluang acak menyerang tiba-tiba jika dalam jarak ideal
        if (distanceToPlayer <= 4f && Random.Range(0, 500) == 5)
        {
            ChangeState(EnemyState.Attack);
            stateTimer = 0.5f; // Ancang-ancang atlet lari
            rb.velocity = Vector2.zero;
            if (anim != null) anim.SetTrigger("TelegraphAttack");
        }
    }

    protected override void UpdateAttackState()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            // SERANGAN TAK TERTEBAK: Melompat menerjang dengan lintasan melengkung parabola ke arah player
            float jumpDir = playerTransform.position.x > transform.position.x ? 1f : -1f;
            rb.velocity = new Vector2(jumpDir * moveSpeed * 2.5f, 6f);
            
            // Masuk kembali ke pola acak setelah mendarat
            ChangeState(EnemyState.Move);
        }
    }
}