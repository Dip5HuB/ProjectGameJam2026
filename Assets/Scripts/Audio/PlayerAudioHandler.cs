using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioHandler : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerController playerController; // [KODE BARU] Referensi ke skrip utama

    [Header("Audio Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip deathSound;

    [Header("Footstep Timer Settings")]
    [SerializeField] private float footstepInterval = 0.35f; 
    private float footstepTimer;

    [Header("Audio Settings")]
    [Range(0f, 1f)] [SerializeField] private float soundVolume = 0.8f;
    [Range(0.1f, 0.5f)] [SerializeField] private float pitchRandomness = 0.1f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // [KODE BARU] Otomatis mengambil skrip PlayerController pada Game Object yang sama
        playerController = GetComponent<PlayerController>(); 
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        HandleFootstepsWithTimer();
    }

    private void HandleFootstepsWithTimer()
    {
        // Pengaman: Jika skrip player tidak ditemukan, hentikan fungsi
        if (playerController == null) return;

        // =====================================================================
        // KUNCI UTAMA SINKRONISASI:
        // Suara langkah kaki HANYA BOLEH berbunyi jika player berada di State MOVE!
        // Jika sedang Attack, Dash, Scary, atau Stagger, suara otomatis terkunci.
        // =====================================================================
        if (playerController.GetCurrentState() == PlayerController.PlayerState.Move)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0)
            {
                PlayFootstepSound();
                footstepTimer = footstepInterval; 
            }
        }
        else
        {
            // Reset timer ke 0 agar saat kembali jalan langsung berbunyi tanpa jeda delay
            footstepTimer = 0f; 
        }
    }

    public void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        int randomIndex = Random.Range(0, footstepSounds.Length);
        PlaySound(footstepSounds[randomIndex]);
    }

    // Fungsi pemicu suara aksi lainnya (Tetap dipanggil lewat Animation Events)
    public void PlayJumpSound() => PlaySound(jumpSound);
    public void PlayDashSound() => PlaySound(dashSound);
    public void PlayAttackSound() => PlaySound(attackSound);
    public void PlayDeathSound() => PlaySound(deathSound);

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        audioSource.pitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
        audioSource.PlayOneShot(clip, soundVolume);
    }
}