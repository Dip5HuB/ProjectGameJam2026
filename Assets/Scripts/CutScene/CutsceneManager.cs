using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Pengaturan Gambar")]
    public Image layarTampil; // Tempat gambar ditampilkan
    public Sprite[] daftarGambar; // Kumpulan gambar cutscene

    [Header("Pindah Scene")]
    public string namaMapSelanjutnya; // Nama map setelah cutscene tamat

    private int indexGambar = 0; // Penghitung urutan gambar

    void Start()
    {
        // Tampilkan gambar pertama (index 0) saat scene dimulai
        if (daftarGambar.Length > 0)
        {
            layarTampil.sprite = daftarGambar[0];
        }
    }

    void Update()
    {
        // Mendeteksi klik kiri mouse atau tap pada layar sentuh
        if (Input.GetMouseButtonDown(0))
        {
            LanjutGambar();
        }
    }

    void LanjutGambar()
    {
        indexGambar++; // Tambah angka urutan

        // Cek apakah masih ada gambar berikutnya?
        if (indexGambar < daftarGambar.Length)
        {
            layarTampil.sprite = daftarGambar[indexGambar];
        }
        else
        {
            // Jika gambar sudah habis, pindah ke map utama
            SceneManager.LoadScene(namaMapSelanjutnya);
        }
    }
}