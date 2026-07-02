using UnityEngine;
using UnityEngine.UI; // Tetap wajib untuk mendeteksi komponen UI

public class StealthUIHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController; // Slot untuk Player
    
    private Slider stealthSlider; // [DIUBAH] Sekarang menggunakan Slider, bukan Image lagi

    private void Awake()
    {
        // Otomatis mengambil komponen Slider yang menempel pada object UI ini
        stealthSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        // Setel batas aman slider dari angka 0 hingga 1
        if (stealthSlider != null)
        {
            stealthSlider.minValue = 0f;
            stealthSlider.maxValue = 1f;
            stealthSlider.value = 1f; // Di awal game, setel penuh
        }
    }

    private void Update()
    {
        // Pengaman: Jika player atau slider belum siap, abaikan
        if (playerController == null || stealthSlider == null) return;

        // TUGAS UTAMA: Masukkan data persentase dari player langsung ke value Slider
        stealthSlider.value = playerController.GetStealthFillAmount();
    }
}