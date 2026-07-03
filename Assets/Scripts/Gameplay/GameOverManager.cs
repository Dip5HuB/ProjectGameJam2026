using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject gameOverPanel; 

    [Header("Dramatic Settings")]
    [Range(0.1f, 0.5f)] [SerializeField] private float slowMoFactor = 0.25f; 
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Nama scene menu utama kamu

    private bool isGameOverTriggered = false;

    private void Start()
    {
        if (gameOverPanel != null) 
            gameOverPanel.SetActive(false);

        // Pastikan waktu dunia kembali normal saat masuk map baru
        Time.timeScale = 1f; 
    }

    private void Update()
    {
        if (playerController == null || isGameOverTriggered) return;

        if (playerController.GetCurrentState() == PlayerController.PlayerState.Dead)
        {
            TriggerGameOverEffects();
        }
    }

    private void TriggerGameOverEffects()
    {
        isGameOverTriggered = true;
        Debug.Log("Player Kalah! Memulai dramatisasi kematian...");

        // 1. EFEK SLOW MOTION
        Time.timeScale = slowMoFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; 

        // 2. MUNCULKAN PANEL GAME OVER
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // =====================================================================
        // KUNCI UTAMA PC GAME: Bebaskan kursor mouse agar bisa diklik pemain!
        // =====================================================================
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // =====================================================================
    // FUNGSI UI BUTTONS
    // =====================================================================
    
    // Dipanggil oleh: Btn_Restart
    public void RestartLevel()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Dipanggil oleh: Btn_MainMenu
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(mainMenuSceneName);
    }
}