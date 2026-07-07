using UnityEngine;
using UnityEngine.SceneManagement; // WAJIB untuk fungsi Restart/Load ulang game

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject gameOverPanel; // Seret Panel UI Game Over kamu ke sini

    [Header("Dramatic Settings")]
    [Range(0.1f, 0.5f)] [SerializeField] private float slowMoFactor = 0.25f; // Semakin kecil, semakin lambat waktunya
    
    private bool isGameOverTriggered = false;

    private void Start()
    {
        // Pengaman awal: Pastikan panel Game Over tidak menutupi layar saat game baru dimulai
        if (gameOverPanel != null) 
            gameOverPanel.SetActive(false);

        // Pastikan waktu dunia berjalan normal kembali (1f) jika pemain melakukan Restart
        Time.timeScale = 1f; 
    }

    private void Update()
    {
        // Jika belum diset atau game over sudah berjalan, abaikan fungsi di bawah
        if (playerController == null || isGameOverTriggered) return;

        // TUGAS UTAMA: Mengintip apakah player sudah ambruk ke state Dead
        if (playerController.GetCurrentState() == PlayerController.PlayerState.Dead)
        {
            TriggerGameOverEffects();
        }
    }

    private void TriggerGameOverEffects()
    {
        isGameOverTriggered = true;
        Debug.Log("Player Kalah! Memulai dramatisasi kematian...");

        // 1. EFEK SLOW MOTION (Waktu dunia melambat drastis ala game AAA)
        Time.timeScale = slowMoFactor;
        
        // Menyelaraskan kestabilan hitungan fisika Unity saat waktu melambat
        Time.fixedDeltaTime = 0.02f * Time.timeScale; 

        // 2. MUNCULKAN PANEL GAME OVER
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // =====================================================================
    // FUNGSI TOMBOL UI (Pasang fungsi ini di Button Restart kamu nanti)
    // =====================================================================
    public void RestartLevel()
    {
        // SANGAT WAJIB: Kembalikan waktu ke normal sebelum memuat ulang map!
        Time.timeScale = 1f; 
        
        // Memuat ulang scene/level yang sedang aktif saat ini dari awal
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}