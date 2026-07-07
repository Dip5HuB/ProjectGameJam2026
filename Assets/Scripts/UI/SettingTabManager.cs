using UnityEngine;

/// <summary>
/// Script ini berfungsi untuk mengontrol perpindahan tab (Audio vs Controls) di dalam menu Setting.
/// Menghubungkan visual button effects agar tab yang sedang aktif tetap tersorot (highlighted).
/// </summary>
public class SettingTabManager : MonoBehaviour
{
    [Header("Tab Button Components")]
    [Tooltip("Komponen MenuButtonEffects dari tombol AUDIO di tab kiri")]
    [SerializeField] private MenuButtonEffects audioTabButton;

    [Tooltip("Komponen MenuButtonEffects dari tombol CONTROLS di tab kiri")]
    [SerializeField] private MenuButtonEffects controlsTabButton;

    [Header("Sub Panel Panels")]
    [Tooltip("Objek Panel pengubah audio (ubah_audio)")]
    [SerializeField] private GameObject audioPanel;

    [Tooltip("Objek Panel pengubah kontrol (controls)")]
    [SerializeField] private GameObject controlsPanel;

    private void Start()
    {
        // Secara default aktifkan tab AUDIO saat pertama kali masuk ke panel setting
        SelectAudioTab();
    }

    /// <summary>
    /// Fungsi untuk mengaktifkan tab AUDIO.
    /// Dipanggil oleh event On Click() pada tombol AUDIO di kiri.
    /// </summary>
    public void SelectAudioTab()
    {
        // 1. Tampilkan sub-panel Audio, sembunyikan sub-panel Controls
        if (audioPanel != null) audioPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        // 2. Kunci visual tombol AUDIO agar tetap menyala (highlighted)
        if (audioTabButton != null) audioTabButton.SetSelectedState(true);
        
        // 3. Matikan kunci visual tombol CONTROLS agar kembali normal
        if (controlsTabButton != null) controlsTabButton.SetSelectedState(false);
    }

    /// <summary>
    /// Fungsi untuk mengaktifkan tab CONTROLS.
    /// Dipanggil oleh event On Click() pada tombol CONTROLS di kiri.
    /// </summary>
    public void SelectControlsTab()
    {
        // 1. Tampilkan sub-panel Controls, sembunyikan sub-panel Audio
        if (audioPanel != null) audioPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);

        // 2. Kunci visual tombol CONTROLS agar tetap menyala (highlighted)
        if (controlsTabButton != null) controlsTabButton.SetSelectedState(true);

        // 3. Matikan kunci visual tombol AUDIO agar kembali normal
        if (audioTabButton != null) audioTabButton.SetSelectedState(false);
    }
}
