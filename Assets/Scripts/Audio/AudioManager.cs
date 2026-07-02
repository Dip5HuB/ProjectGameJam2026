using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource bgmSource;

    [Header("Global Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;

    // Menyimpan data offset volume lagu dari scene yang sedang aktif saat ini
    private float currentSceneBgmOffset = 1f;

    private void Awake()
    {
        // Sistem Singleton: Menjaga agar hanya ada 1 AudioManager di dalam game
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Jangan hancurkan objek ini saat pindah scene

            // Otomatis membuat komponen AudioSource internal khusus untuk BGM
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;

            // Memuat data volume yang terakhir kali disimpan oleh pemain
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        }
        else
        {
            Destroy(gameObject); // Hancurkan jika ada duplikat di scene baru
        }
    }

    // FUNGSI UTAMA: Dipanggil oleh scene untuk mengganti lagu & volume bawaan scene tersebut
    public void PlaySceneBGM(AudioClip newTrack, float sceneVolumeOffset)
    {
        currentSceneBgmOffset = sceneVolumeOffset;

        // Jika lagu yang ingin diputar sama dengan yang sedang berjalan, abaikan agar tidak mengulang dari awal
        if (bgmSource.clip == newTrack)
        {
            UpdateActualVolume();
            return;
        }

        bgmSource.clip = newTrack;

        if (newTrack != null)
        {
            bgmSource.Play();
        }
        else
        {
            bgmSource.Stop();
        }

        UpdateActualVolume();
    }

    // FUNGSI UTAMA: Dipanggil oleh Slider UI untuk mengubah volume global
    public void SetMasterVolume(float newVolume)
    {
        masterVolume = Mathf.Clamp01(newVolume);
        
        // Simpan setelan ke memori agar saat game dibuka lagi, volumenya tidak reset
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();

        UpdateActualVolume();
    }

    // Rumus matematika menyelaraskan volume global dengan keunikan volume tiap scene
    private void UpdateActualVolume()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = currentSceneBgmOffset * masterVolume;
        }
    }

    // Getter untuk dibaca oleh Slider UI saat menu setting dibuka
    public float GetMasterVolume() => masterVolume;
}