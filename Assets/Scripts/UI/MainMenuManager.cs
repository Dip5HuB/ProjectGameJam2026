using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script ini berfungsi sebagai pusat kontrol menu utama (Main Menu Manager).
/// Mengatur navigasi antar panel UI (Menu Utama, Setting, Credits) serta transisi scene ke game utama.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panel References")]
    [Tooltip("Panel menu utama yang menampung tombol Play, Setting, dll. (Panel_Menu)")]
    [SerializeField] private GameObject menuPanel;

    [Tooltip("Panel untuk pengaturan game (Panel_setting)")]
    [SerializeField] private GameObject settingPanel;

    [Tooltip("Panel untuk melihat kredit pembuat game (Panel_Credits)")]
    [SerializeField] private GameObject creditsPanel;

    [Tooltip("Panel untuk memilih stage/level (jika ada di kemudian hari)")]
    [SerializeField] private GameObject stagePanel;

    [Header("Scene Settings")]
    [Tooltip("Nama scene yang akan dimuat ketika player menekan tombol Play (Contoh: Map_1)")]
    [SerializeField] private string playSceneName = "Map_1";

    private void Start()
    {
        // Pastikan saat game dimulai, hanya menuPanel yang aktif sedangkan panel lain dinonaktifkan.
        ShowPanel(menuPanel);
    }

    /// <summary>
    /// Fungsi untuk memulai permainan dengan memuat scene target (misalnya Map_1).
    /// Dipanggil oleh event On Click() pada btn_play.
    /// </summary>
    public void PlayGame()
    {
        Debug.Log("Memulai permainan... Memuat scene: " + playSceneName);
        
        // Memuat scene game berdasarkan nama scene yang dimasukkan di Inspector
        SceneManager.LoadScene(playSceneName);
    }

    /// <summary>
    /// Fungsi untuk membuka panel Pengaturan (Setting).
    /// Dipanggil oleh event On Click() pada Btn_setting.
    /// </summary>
    public void OpenSetting()
    {
        ShowPanel(settingPanel);
    }

    /// <summary>
    /// Fungsi untuk membuka panel Kredit (Credits).
    /// Dipanggil oleh event On Click() pada Btn_credits.
    /// </summary>
    public void OpenCredits()
    {
        ShowPanel(creditsPanel);
    }

    /// <summary>
    /// Fungsi untuk membuka panel Pemilihan Stage (Stage).
    /// Dipanggil oleh event On Click() pada Btn_stage.
    /// </summary>
    public void OpenStage()
    {
        if (stagePanel != null)
        {
            ShowPanel(stagePanel);
        }
        else
        {
            Debug.LogWarning("Panel Stage belum dimasukkan ke Inspector! Silakan assign objek panel stage terlebih dahulu.");
        }
    }

    /// <summary>
    /// Fungsi untuk kembali ke Panel Menu Utama dari panel sub-menu apa pun (Back Button).
    /// Dipanggil oleh tombol "Kembali / Back" yang ada di dalam panel setting atau credits.
    /// </summary>
    public void BackToMainMenu()
    {
        ShowPanel(menuPanel);
    }

    /// <summary>
    /// Fungsi pembantu untuk mengaktifkan satu panel tertentu dan menonaktifkan panel lainnya.
    /// Menjamin kerapihan status panel UI agar tidak saling tumpang tindih.
    /// </summary>
    /// <param name="targetPanel">GameObject panel yang ingin ditampilkan</param>
    private void ShowPanel(GameObject targetPanel)
    {
        // Nonaktifkan semua panel terlebih dahulu jika referensinya ada
        if (menuPanel != null) menuPanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (stagePanel != null) stagePanel.SetActive(false);

        // Aktifkan panel target yang diinginkan
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Fungsi untuk keluar dari game.
    /// Dipanggil oleh event On Click() pada Btn_exit.
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Keluar dari game...");

        // Fungsi ini akan menutup aplikasi jika game sudah di-build (Standalone Build)
        Application.Quit();

        // Di dalam Unity Editor, Application.Quit() tidak akan menghentikan mode Play.
        // Oleh karena itu, kita tambahkan baris berikut agar bisa ditest di Editor.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
