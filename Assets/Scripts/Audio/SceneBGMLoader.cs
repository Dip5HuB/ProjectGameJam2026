using UnityEngine;

public class SceneBGMLoader : MonoBehaviour
{
    [Header("Scene Audio Configuration")]
    [SerializeField] private AudioClip sceneBGMClip; // Masukkan file audio khusus scene ini
    
    [Tooltip("Volume dasar khusus scene ini (Misal: Main Menu dibuat 1, tapi Map 0 horor dibuat 0.5 saja)")]
    [Range(0f, 1f)] [SerializeField] private float sceneVolumeOffset = 1f;

    private void Start()
    {
        // Cari AudioManager pusat, lalu setorkan lagu khusus milik scene ini
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySceneBGM(sceneBGMClip, sceneVolumeOffset);
        }
        else
        {
            Debug.LogWarning("AudioManager pusat belum diletakkan di scene awal (Main Menu)!");
        }
    }
}