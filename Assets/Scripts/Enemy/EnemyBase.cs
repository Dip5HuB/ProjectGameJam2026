using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public enum EnemyState { Idle, Move, Attack, Dead }

    [Header("Base Enemy Stats")]
    [SerializeField] protected int maxHealth = 50;
    protected int currentHealth;
    [SerializeField] protected EnemyState currentState = EnemyState.Idle;
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Player Detection")]
    [SerializeField] protected float aggroRange = 5f; // jarak musuh melihat player
    [SerializeField] protected LayerMask playerLayer;
    protected Transform playerTransform;

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

        if (currentHealth <= 0) Die();
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