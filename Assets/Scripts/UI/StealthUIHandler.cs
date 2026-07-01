using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StealthUIHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController; // Slot untuk menarik object Player
    
    private Image uiImage; // Menampung komponen Image dari object UI ini sendiri

    private void Awake()
    {
        // Otomatis mengambil komponen Image yang menempel pada object UI ini
        uiImage = GetComponent<Image>();
    }

    private void Start()
    {
        // Pastikan di awal game fill dalam kondisi penuh siap pakai
        if (uiImage != null)
        {
            uiImage.fillAmount = 1f;
        }
    }

    private void Update()
    {
        // Pengaman: Jika player atau image belum terpasang, abaikan agar tidak error
        if (playerController == null || uiImage == null) return;

        // TUGAS UTAMA: Mengambil data persentase dari player dan memasukkannya ke visual Fill Amount
        uiImage.fillAmount = playerController.GetStealthFillAmount();
    }
}
