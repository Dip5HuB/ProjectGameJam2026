using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PindahLevel : MonoBehaviour
{
    public string namaMapTujuan;
    [Header("Nama Titik Muncul di Map Tujuan:")]
    public string namaTitikSpawn; // Titik tempat Gojo akan muncul di map berikutnya
    
    public Image layarHitam;
    public float waktuTransisi = 1f;

    // Variabel static ini berfungsi sebagai "memori" yang tidak akan terhapus saat pindah map
    public static string memoriSpawn = "";

    private void Start()
    {
        if (layarHitam != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Simpan nama titik tujuan ke memori sebelum layar memudar
            memoriSpawn = namaTitikSpawn;
            StartCoroutine(FadeOutDanPindah());
        }
    }

    IEnumerator FadeIn()
    {
        layarHitam.gameObject.SetActive(true);
        Color warna = layarHitam.color;
        warna.a = 1f;
        layarHitam.color = warna;

        while (warna.a > 0f)
        {
            warna.a -= Time.deltaTime / waktuTransisi;
            layarHitam.color = warna;
            yield return null;
        }
        layarHitam.gameObject.SetActive(false);
    }

    IEnumerator FadeOutDanPindah()
    {
        layarHitam.gameObject.SetActive(true);
        Color warna = layarHitam.color;
        warna.a = 0f;

        while (warna.a < 1f)
        {
            warna.a += Time.deltaTime / waktuTransisi;
            layarHitam.color = warna;
            yield return null;
        }

        SceneManager.LoadScene(namaMapTujuan);
    }
}