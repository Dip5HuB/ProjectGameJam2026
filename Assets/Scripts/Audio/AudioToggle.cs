using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script ini berfungsi untuk mengontrol tombol ON/OFF pada menu pengaturan audio.
/// Mengatur pergantian sprite (menyala biru/merah atau abu-abu nonaktif) dan menyimpan statusnya di PlayerPrefs.
/// </summary>
public class AudioToggle : MonoBehaviour
{
    [Header("UI Button Images")]
    [Tooltip("Komponen Image dari tombol ON (on_master_volum, dll)")]
    [SerializeField] private Image onButtonImage;

    [Tooltip("Komponen Image dari tombol OFF (off_master_volum, dll)")]
    [SerializeField] private Image offButtonImage;

    [Header("Sprites List")]
    [Tooltip("Sprite tombol ON saat aktif (Menyala Biru - Setting Audio_63)")]
    [SerializeField] private Sprite activeOnSprite;

    [Tooltip("Sprite tombol ON saat nonaktif (Abu-abu ON - Setting Audio_36)")]
    [SerializeField] private Sprite inactiveOnSprite;

    [Tooltip("Sprite tombol OFF saat aktif (Menyala Merah - Setting Audio_37)")]
    [SerializeField] private Sprite activeOffSprite;

    [Tooltip("Sprite tombol OFF saat nonaktif (Abu-abu OFF)")]
    [SerializeField] private Sprite inactiveOffSprite;

    [Header("Settings Keys")]
    [Tooltip("Kata kunci unik untuk menyimpan status audio ini di PlayerPrefs (contoh: MasterVolume, MusicVolume, SFXVolume)")]
    [SerializeField] private string prefKey = "MasterVolume";

    // Status toggle saat ini (true = ON, false = OFF)
    public bool IsOn { get; private set; }

    private void Start()
    {
        // Memuat status toggle terakhir dari PlayerPrefs. Default-nya adalah ON (bernilai 1) jika belum pernah diset.
        bool savedState = PlayerPrefs.GetInt(prefKey, 1) == 1;
        
        // Atur tampilan tombol sesuai status yang dimuat
        SetState(savedState);
    }

    /// <summary>
    /// Fungsi untuk menyetel status audio ke ON.
    /// Dipanggil melalui event On Click() pada tombol ON.
    /// </summary>
    public void TurnOn()
    {
        if (!IsOn)
        {
            SetState(true);
            // Simpan status ON (1) ke PlayerPrefs
            PlayerPrefs.SetInt(prefKey, 1);
            PlayerPrefs.Save();

            // Sampaikan perubahan ke sistem (misalnya memanggil fungsi audio manager)
            ApplyAudioSettings();
        }
    }

    /// <summary>
    /// Fungsi untuk menyetel status audio ke OFF.
    /// Dipanggil melalui event On Click() pada tombol OFF.
    /// </summary>
    public void TurnOff()
    {
        if (IsOn)
        {
            SetState(false);
            // Simpan status OFF (0) ke PlayerPrefs
            PlayerPrefs.SetInt(prefKey, 0);
            PlayerPrefs.Save();

            // Sampaikan perubahan ke sistem (misalnya memanggil fungsi audio manager)
            ApplyAudioSettings();
        }
    }

    /// <summary>
    /// Mengubah status internal dan memperbarui tampilan sprite tombol secara visual.
    /// </summary>
    /// <param name="state">True untuk ON (Aktif), False untuk OFF (Nonaktif)</param>
    private void SetState(bool state)
    {
        IsOn = state;

        if (IsOn)
        {
            // Jika ON:
            // Tombol ON menyala biru
            if (onButtonImage != null && activeOnSprite != null)
                onButtonImage.sprite = activeOnSprite;

            // Tombol OFF mati (abu-abu)
            if (offButtonImage != null && inactiveOffSprite != null)
                offButtonImage.sprite = inactiveOffSprite;
        }
        else
        {
            // Jika OFF:
            // Tombol ON mati (abu-abu)
            if (onButtonImage != null && inactiveOnSprite != null)
                onButtonImage.sprite = inactiveOnSprite;

            // Tombol OFF menyala merah
            if (offButtonImage != null && activeOffSprite != null)
                offButtonImage.sprite = activeOffSprite;
        }
    }

    /// <summary>
    /// Tempat untuk mengintegrasikan efek volume sebenarnya ke game.
    /// Anda dapat menyesuaikan fungsi ini untuk menyetel AudioMixer atau AudioListener.
    /// </summary>
    private void ApplyAudioSettings()
    {
        Debug.Log($"Pengaturan {prefKey} diubah menjadi: {(IsOn ? "ON" : "OFF")}");

        // Contoh implementasi sederhana: Jika ini MasterVolume, kita bisa mute global
        if (prefKey == "MasterVolume")
        {
            AudioListener.volume = IsOn ? 1f : 0f;
        }
        
        // TODO: Hubungkan dengan AudioManager Anda di sini jika ada.
    }
}
