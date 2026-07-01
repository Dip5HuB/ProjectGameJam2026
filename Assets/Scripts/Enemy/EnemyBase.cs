using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public enum EnemyState { Idle, Move, Attack, Stagger,Dead }

    [Header("Base Enemy Stats")]
    [SerializeField] protected int maxHealth = 50;
    protected int currentHealth;
    [SerializeField] protected EnemyState currentState = EnemyState.Idle;
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Player Detection")]
    [SerializeField] protected float aggroRange = 5f; // jarak musuh melihat player
    [SerializeField] protected LayerMask playerLayer;
    protected Transform playerTransform;

    [Header("Stagger & Knockback Settings")]
    [SerializeField] protected float staggerDuration = 0.35f;
    [SerializeField] protected float knockbackForceX = 5f;
    [SerializeField] protected float knockbackForceY = 3f;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected ItemDropSystem dropSystem;
    protected bool isDead = false;
    protected float stateTimer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        dropSystem = GetComponent<ItemDropSystem>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        // Otomatis mencari objek dengan tag Player di dalam map
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    protected virtual void Update()
    {
        if (isDead) return;

        // Logika FSM otomatis yang akan dijalankan oleh anak-anak hantu
        switch (currentState)
        {
            case EnemyState.Idle: UpdateIdleState(); break;
            case EnemyState.Move: UpdateMoveState(); break;
            case EnemyState.Attack: UpdateAttackState(); break;
            case EnemyState.Stagger: UpdateStaggerState(); break; // [TAMBAHKAN INI]
        }
    }

    // Mengatur pergerakan hantu selama masa pusing akibat sabetan sarung
    protected virtual void UpdateStaggerState()
    {
        stateTimer -= Time.deltaTime;

        if (rb != null)
        {
            // Trik Pengaman khusus Kuntilanak: Karena Kuntilanak melayang (tidak punya gravitasi),
            // kita harus mengerem sumbu Y-nya juga agar dia tidak melayang ke atas langit selamanya saat kena hit.
            if (rb.gravityScale == 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x * 0.85f, rb.velocity.y * 0.85f);
            }
            else
            {
                // Untuk hantu darat (Pocong, Genderuwo, Tuyul), cukup rem gesekan horizontalnya saja
                rb.velocity = new Vector2(rb.velocity.x * 0.85f, rb.velocity.y);
            }
        }

        // Jika waktu pusingnya sudah habis, bangunkan hantu kembali ke mode normal
        if (stateTimer <= 0)
        {
            // Jika player masih dekat, langsung emosi mengejar (Move), jika jauh kembali diam (Idle)
            ChangeState(IsPlayerInAggroRange() ? EnemyState.Move : EnemyState.Idle);
        }
    }

    // FUNGSI VIRTUAL: Bisa diubah isinya secara unik di skrip masing-masing hantu
    protected virtual void UpdateIdleState() { }
    protected virtual void UpdateMoveState() { }
    protected virtual void UpdateAttackState() { }

    public virtual void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} Kena Hit! Sisa HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Jika masih hidup, berikan efek knockback ke musuh
        if (playerTransform != null)
        {
            // Tentukan arah: jika posisi hantu lebih besar dari player, dorong ke kanan (1), jika tidak ke kiri (-1)
            float knockbackDir = transform.position.x > playerTransform.position.x ? 1f : -1f;

            // Berikan gaya dorong instan ke Rigidbody2D musuh
            if (rb != null)
            {
                rb.velocity = new Vector2(knockbackDir * knockbackForceX, knockbackForceY);
            }
        }

        // Pindahkan state hantu ke Stagger dan nyalakan timernya
        stateTimer = staggerDuration;
        ChangeState(EnemyState.Stagger);

        // Mainkan animasi terkejut/stagger di Animator hantu (jika ada parameternya)
        if (anim != null) anim.SetTrigger("Stagger");
    }

    protected virtual void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        
        if (anim != null) anim.SetTrigger("Dead"); // Memicu animasi mengempis/asap

        if (dropSystem != null) dropSystem.DropRandomItem(transform.position);

        Destroy(gameObject, 0.5f); // Beri jeda 0.5 detik agar animasi kalah selesai diputar
    }

    // Helper untuk mengecek apakah player berada di dalam jarak pandang musuh
    protected bool IsPlayerInAggroRange()
    {
        if (playerTransform == null) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= aggroRange;
    }
}