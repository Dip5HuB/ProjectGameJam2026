using UnityEngine;
using UnityEngine.InputSystem; // WAJIB karena proyekmu menggunakan Input System Baru
using UnityEngine.SceneManagement; // Digunakan untuk membedakan scene menu dan gameplay

public class SettingsToggleHandler : MonoBehaviour
{
    [Header("UI Panel Reference")]
    [SerializeField] private GameObject settingsPanel; // Tarik objek isi panel setting ke sini

    [Header("Navigation Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Sesuaikan dengan nama scene Main Menu kamu

    private bool isPanelActive = false;

    private void Start()
    {
        // Pengaman: Saat awal masuk map, pastikan panel dalam kondisi tertutup
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            isPanelActive = false;
        }
    }

    private void Update()
    {
        // Fitur Direct Input: Mendeteksi apakah tombol ESC di keyboard ditekan pada frame ini
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettingsMenu();
        }
    }

    public void ToggleSettingsMenu()
    {
        if (settingsPanel == null) return;

        // Balikkan status (Jika sedang buka jadi tutup, jika sedang tutup jadi buka)
        isPanelActive = !isPanelActive;
        settingsPanel.SetActive(isPanelActive);

        // =====================================================================
        // MECHANIC BONUS: FITUR PAUSE GAME
        // =====================================================================
        // Kita cek, jika scene aktif SAAT INI BUKAN bernama "MainMenu", 
        // maka game akan otomatis berhenti (Freeze) saat menu dibuka agar tidak digebuk hantu.
        if (SceneManager.GetActiveScene().name != "MainMenu") // <--- Sesuaikan nama Scene Main Menu kamu
        {
            // Jika panel terbuka, waktu dunia jadi 0 (Pause). Jika ditutup, kembali normal (1)
            Time.timeScale = isPanelActive ? 0f : 1f;
        }
    }

    public void BackToMainMenuScene()
    {
        // PENGAMAN UTAMA: Kembalikan waktu dunia ke normal sebelum kabur dari map!
        Time.timeScale = 1f; 

        Debug.Log("Keluar dari map gameplay... Kembali ke " + mainMenuSceneName);
        
        // Pindah scene ke menu utama
        SceneManager.LoadScene(mainMenuSceneName);
    }
}