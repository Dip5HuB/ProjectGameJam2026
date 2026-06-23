using UnityEngine;

public class EnemyPocong : EnemyBase
{
    [Header("Pocong Specific Mechanics")]
    [SerializeField] private float jumpForceX = 4f;
    [SerializeField] private float jumpForceY = 5f;
    [SerializeField] private float hopDelay = 1.5f; // Jeda waktu ritmis antar lompatan
    private float hopTimer;
    private bool isHoppping = false;

    [Header("Ground Check Pocong")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    protected override void UpdateIdleState()
    {
        // Pocong bersandar tegak lurus di sudut, jika player mendekat, masuk ke Chase Behavior (Move)
        if (IsPlayerInAggroRange())
        {
            ChangeState(EnemyState.Move);
        }
    }

    protected override void UpdateMoveState()
    {
        if (playerTransform == null) return;

        // Hitung arah menuju player (Kanan = 1, Kiri = -1)
        float direction = playerTransform.position.x > transform.position.x ? 1f : -1f;

        // Logika Lompat Ritmis (Jump -> Land -> Jeda)
        hopTimer -= Time.deltaTime;
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (isGrounded && !isHoppping && hopTimer <= 0)
        {
            // Eksekusi Lompat menuju player
            isHoppping = true;
            rb.velocity = new Vector2(direction * jumpForceX, jumpForceY);
            if (anim != null) anim.SetTrigger("Jump");
        }

        // Jika baru saja mendarat, aktifkan jeda ritmis sebelum lompat lagi
        if (isGrounded && isHoppping && rb.velocity.y <= 0.1f)
        {
            isHoppping = false;
            hopTimer = hopDelay; // Setel waktu tunggu santai sebelum melompat kembali
            rb.velocity = new Vector2(0, rb.velocity.y); // Hentikan luncuran x saat mendarat
        }

        // Jika jarak ke player sudah sangat dekat, masuk ke Telegraph & Execution (Attack State)
        if (Vector2.Distance(transform.position, playerTransform.position) <= 2f)
        {
            ChangeState(EnemyState.Attack);
            stateTimer = 0.5f; // Waktu ancang-ancang atlet lari sebelum melesat kilat
            rb.velocity = Vector2.zero;
        }
    }

    protected override void UpdateAttackState()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            // EKSEKUSI SERANGAN KILAT: Melesat kencang ke arah player
            float dashDir = playerTransform.position.x > transform.position.x ? 1f : -1f;
            rb.velocity = new Vector2(dashDir * moveSpeed * 3f, rb.velocity.y);
            
            // Kembali ke move state setelah menerjang beberapa saat
            ChangeState(EnemyState.Move);
        }
    }
}