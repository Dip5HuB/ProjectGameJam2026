using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioHandler : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerController playerController; 

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
        playerController = GetComponent<PlayerController>(); 
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        HandleFootstepsWithTimer();
    }

    private void HandleFootstepsWithTimer()
    {
        if (playerController == null) return;

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
            footstepTimer = 0f; 
        }
    }

    public void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        int randomIndex = Random.Range(0, footstepSounds.Length);
        PlaySound(footstepSounds[randomIndex]);
    }

    public void PlayJumpSound() => PlaySound(jumpSound);
    public void PlayDashSound() => PlaySound(dashSound);
    public void PlayAttackSound() => PlaySound(attackSound);
    public void PlayDeathSound() => PlaySound(deathSound);

    // =====================================================================
    // BAGIAN YANG DIPERBAIKI: Menghubungkan SFX ke Master Volume Global
    // =====================================================================
    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        // Variasi pitch alami agar suara tidak monoton
        audioSource.pitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);

        // KUNCI UTAMA: Kalibrasi Volume Lokal Karakter dikali dengan Master Volume Global
        float calculatedVolume = soundVolume;
        if (AudioManager.Instance != null)
        {
            calculatedVolume = soundVolume * AudioManager.Instance.GetMasterVolume();
        }

        // Jalankan audio dengan volume hasil kalibrasi terbaru
        audioSource.PlayOneShot(clip, calculatedVolume);
    }
}