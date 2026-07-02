using UnityEngine;
using UnityEngine.UI; // WAJIB untuk mendeteksi komponen Slider

public class VolumeSliderHandler : MonoBehaviour
{
    private Slider volumeSlider;

    private void Awake()
    {
        volumeSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (volumeSlider == null || AudioManager.Instance == null) return;

        // Atur agar batas min-max slider berada di angka 0 sampai 1
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        // Set posisi handle slider sesuai dengan data volume terakhir yang diingat AudioManager
        volumeSlider.value = AudioManager.Instance.GetMasterVolume();

        // Mendaftarkan fungsi gerak slider secara otomatis tanpa setup manual di Inspector
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}