using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // DAFTAR STATE
    public enum PlayerState
    {
        Idle,
        Move,
        Airborne,
        Attack,
        Dash,
        Stealth,
        Stagger,
        Dead
    }

    [Header("Current State")]
    [SerializeField] private PlayerState currentState = PlayerState.Idle;

    [Header("Combat & Mechanics")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float attackDuration = 0.35f;
    [SerializeField] private float staggerDuration = 0.4f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    // Komponen Internal
    private Rigidbody2D rb;
    private Animator anim;
    private GameInput gameInput;
    
    // Variabel Pendukung Logika
    private float horizontalInput;
    private bool isGrounded;
    private float stateTimer;
    private bool isFacingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gameInput = new GameInput();
    }

    private void OnEnable()
    {
        gameInput.Player.Enable();
        
        gameInput.Player.Jump.performed += ctx => OnJumpInput();
        gameInput.Player.Dash.performed += ctx => OnDashInput();
        gameInput.Player.Attack.performed += ctx => OnAttackInput();
        gameInput.Player.Stealth.performed += ctx => OnStealthInput();
    }

    private void OnDisable() => gameInput.Player.Disable();

    private void Start()
    {
        currentHealth = maxHealth;
        ChangeState(PlayerState.Idle);
    }

    private void Update()
    {
        // 1. Selalu cek ground di setiap frame
        isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);

        // 2. Berikan data isGrounded dan isRunning ke Animator di setiap frame
        anim.SetBool("isGrounded", isGrounded);
        
        // Membaca input jalan (A/D)
        horizontalInput = gameInput.Player.Move.ReadValue<Vector2>().x;
        
        anim.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0.1f);

        FlipController();

        // LOGIKA UPDATE BERDASARKAN STATE
        switch (currentState)
        {
            case PlayerState.Idle:
                if (Mathf.Abs(horizontalInput) > 0.1f) ChangeState(PlayerState.Move);
                if (!isGrounded && rb.velocity.y < -0.1f) ChangeState(PlayerState.Airborne);
                break;

            case PlayerState.Move:
                if (Mathf.Abs(horizontalInput) < 0.1f) ChangeState(PlayerState.Idle);
                if (!isGrounded) ChangeState(PlayerState.Airborne);
                break;

            case PlayerState.Airborne:
                // Jika sudah menapak tanah kembali, tentukan apakah ke Idle atau Move
                if (isGrounded && Mathf.Abs(rb.velocity.y) < 0.1f)
                {
                    ChangeState(Mathf.Abs(horizontalInput) > 0.1f ? PlayerState.Move : PlayerState.Idle);
                }
                break;

            case PlayerState.Attack:
            case PlayerState.Dash:
            case PlayerState.Stagger:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    ChangeState(PlayerState.Idle);
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        // JALANKAN FISIKA BERDASARKAN STATE AKTIF
        switch (currentState)
        {
            case PlayerState.Idle:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;

            case PlayerState.Move:
            case PlayerState.Airborne:
            case PlayerState.Stealth:
                rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
                break;

            case PlayerState.Dash:
                float dashDir = isFacingRight ? 1 : -1;
                rb.velocity = new Vector2(dashDir * dashSpeed, 0); // Kunci sumur Y agar melesat lurus
                break;

            case PlayerState.Attack:
            case PlayerState.Stagger:
            case PlayerState.Dead:
                // Kurangi/hentikan pergerakan saat mengeksekusi state ini
                rb.velocity = new Vector2(rb.velocity.x * 0.9f, rb.velocity.y);
                break;
        }
    }

    // 2. FUNGSI UNTUK PINDAH STATE (Mengatur transisi awal masuk)
    public void ChangeState(PlayerState newState)
    {
        if (currentState == PlayerState.Dead) return;

        // Reset kecepatan animasi ke normal setiap ganti state
        anim.speed = 1f; 

        currentState = newState;

        switch (currentState)
        {
            case PlayerState.Idle:
                // Transisi diatur otomatis oleh parameter isRunning & isGrounded di Update
                break;

            case PlayerState.Move:
                // Transisi diatur otomatis oleh parameter isRunning & isGrounded di Update
                break;

            case PlayerState.Airborne:
                // Tidak perlu memanggil anim.Play jika transisi "isGrounded = false" sudah diatur di Animator
                break;

            case PlayerState.Attack:
                anim.SetTrigger("Attack");
                stateTimer = attackDuration;
                break;

            case PlayerState.Dash:
                anim.SetTrigger("Dash"); // Picu trigger Dash di kedua layer
                anim.speed = 2f;         // Percepat pemutaran animasi agar terasa melesat
                stateTimer = dashDuration;
                break;

            case PlayerState.Stagger:
                anim.SetTrigger("Stagger"); // Picu animasi terkejut/hit
                stateTimer = staggerDuration;
                break;

            case PlayerState.Dead:
                anim.SetTrigger("Dead");
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
                break;
        }
    }

    // 3. LOGIKA KETIKA TOMBOL INPUT DI TEKAN
    private void OnJumpInput()
    {
        // Berdasarkan FSM: Hanya boleh lompat saat di posisi Idle, Move, atau Stealth
        if ((currentState == PlayerState.Idle || currentState == PlayerState.Move || currentState == PlayerState.Stealth) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            ChangeState(PlayerState.Airborne);
        }
    }

    private void OnDashInput()
    {
        // Berdasarkan FSM: Boleh dash dari hampir semua state (termasuk Cancel Attack)
        if (currentState != PlayerState.Stagger && currentState != PlayerState.Dead && currentState != PlayerState.Dash)
        {
            ChangeState(PlayerState.Dash);
        }
    }

    private void OnAttackInput()
    {
        // Berdasarkan FSM: Menyerang otomatis membatalkan status Stealth
        if (currentState == PlayerState.Idle || currentState == PlayerState.Move || currentState == PlayerState.Stealth || currentState == PlayerState.Airborne)
        {
            ChangeState(PlayerState.Attack);
        }
    }

    private void OnStealthInput()
    {
        if (currentState == PlayerState.Idle || currentState == PlayerState.Move)
        {
            ChangeState(PlayerState.Stealth);
            Debug.Log("Masuk ke Stealth");
        }
        else if (currentState == PlayerState.Stealth)
        {
            // Jika ditekan lagi saat mode stealth, kembali ke normal
            ChangeState(PlayerState.Idle);
            Debug.Log("Keluar dari Stealth");
        }
    }

    // Fungsi luar untuk dipanggil oleh script Musuh/Damage ketika player terkena Hit
    public void TakeDamage(int damage)
    {
        // Proteksi: Jika player sudah mati, abaikan damage berikutnya
        if (currentState == PlayerState.Dead) return;

        // 1. Kurangi darah player
        currentHealth -= damage;
        Debug.Log($"Player terkena hit! Sisa darah: {currentHealth}");

        // 2. CEK KONDISI DARAH HABIS ATAU MASIH ADA
        if (currentHealth <= 0)
        {
            currentHealth = 0;

            // KONDISI MATI: Berikan efek dorongan vertikal/horizontal kecil saat ambruk (opsional)
            float dieKnockbackDir = isFacingRight ? -1f : 1f;
            rb.velocity = new Vector2(dieKnockbackDir * 3f, 5f); // Sedikit terlempar lalu jatuh

            ChangeState(PlayerState.Dead); 
        }
        else
        {
            // KONDISI STAGGER (Darah masih ada): Logika Fallback/Knockback
            // Jika karakter menghadap kanan, dorong ke kiri (-1). Jika menghadap kiri, dorong ke kanan (1).
            float fallbackDirection = isFacingRight ? -1f : 1f;
            
            // Atur kekuatan fallback (Contoh: Mundur horizontal = 6f, Melompat sedikit karena efek benturan = 4f)
            rb.velocity = new Vector2(fallbackDirection * 6f, 4f);

            ChangeState(PlayerState.Stagger); 
        }
    }

    private void FlipController()
    {
        if (currentState == PlayerState.Dash || currentState == PlayerState.Stagger || currentState == PlayerState.Dead) return;

        if (horizontalInput > 0.1f && !isFacingRight) Flip();
        else if (horizontalInput < -0.1f && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
    }
}