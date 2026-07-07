using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;
using UnityEngine.SceneManagement;

public class VisualNovelManager : MonoBehaviour
{
    [Header("Komponen UI")]
    public Image layarGambar;
    public TextMeshProUGUI teksDialog;
    public Image layarHitamTransisi; // Tempat menaruh layar hitam

    [Header("Data Cutscene")]
    public Sprite[] daftarGambar;
    [TextArea(3, 5)] 
    public string[] daftarKalimat; 
    
    [Header("Kecepatan & Waktu")]
    public float kecepatanKetik = 0.04f; 
    public float jedaSetelahTeksSelesai = 2.5f; 
    public float waktuFade = 1f; // Lama waktu layar memudar (1 detik)

    [Header("Pindah Scene")]
    public string namaMapSelanjutnya;

    private int index = 0;

    void Start()
    {
        // Pastikan layar hitam menyala saat mulai
        if (layarHitamTransisi != null)
        {
            layarHitamTransisi.gameObject.SetActive(true);
        }
        
        StartCoroutine(MulaiCutscene());
    }

    IEnumerator MulaiCutscene()
    {
        // Siapkan gambar pertama
        if (daftarGambar.Length > 0) layarGambar.sprite = daftarGambar[0];
        teksDialog.text = "";

        // Efek Fade In awal (Hitam ke Terang)
        yield return StartCoroutine(EfekFade(1f, 0f));

        // Mulai mengetik teks
        StartCoroutine(KetikTeksOtomatis());
    }

    IEnumerator KetikTeksOtomatis()
    {
        teksDialog.text = "";
        
        foreach (char huruf in daftarKalimat[index].ToCharArray())
        {
            teksDialog.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik); 
        }
        
        // Diam sejenak agar pemain bisa membaca
        yield return new WaitForSeconds(jedaSetelahTeksSelesai); 
        
        // Lakukan transisi ke gambar berikutnya
        StartCoroutine(TransisiKeFrameBerikutnya());
    }

    IEnumerator TransisiKeFrameBerikutnya()
    {
        if (index < daftarKalimat.Length - 1)
        {
            // Layar memudar jadi hitam
            yield return StartCoroutine(EfekFade(0f, 1f));

            // Ganti ke gambar berikutnya & kosongkan teks
            index++;
            layarGambar.sprite = daftarGambar[index];
            teksDialog.text = ""; 

            // Layar kembali terang
            yield return StartCoroutine(EfekFade(1f, 0f));

            // Mulai ngetik kalimat baru
            StartCoroutine(KetikTeksOtomatis());
        }
        else
        {
            // Cutscene tamat, layar memudar jadi hitam lalu pindah map
            yield return StartCoroutine(EfekFade(0f, 1f));
            SceneManager.LoadScene(namaMapSelanjutnya);
        }
    }

    // Fungsi khusus untuk mengatur warna layar memudar
    IEnumerator EfekFade(float alphaAwal, float alphaTujuan)
    {
        if (layarHitamTransisi == null) yield break;

        layarHitamTransisi.gameObject.SetActive(true);
        Color warna = layarHitamTransisi.color;
        warna.a = alphaAwal;
        layarHitamTransisi.color = warna;

        float waktuBerjalan = 0f;
        while (waktuBerjalan < waktuFade)
        {
            waktuBerjalan += Time.deltaTime;
            warna.a = Mathf.Lerp(alphaAwal, alphaTujuan, waktuBerjalan / waktuFade);
            layarHitamTransisi.color = warna;
            yield return null;
        }

        warna.a = alphaTujuan;
        layarHitamTransisi.color = warna;

        // Matikan objek layar hitam jika sudah tembus pandang sepenuhnya
        if (alphaTujuan == 0f)
        {
            layarHitamTransisi.gameObject.SetActive(false); 
        }
    }
}