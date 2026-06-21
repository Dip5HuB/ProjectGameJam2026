using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;

    [Header("Drop Prefabs Item")]
    [SerializeField] private GameObject healthDropPrefab;  // Es Cekek
    [SerializeField] private GameObject hastePrefab;       // Sandal Swallow
    [SerializeField] private GameObject berserkPrefab;     // Kopi Bapak
    [SerializeField] private GameObject aegisPrefab;       // Peci Haji

    private void Start()
    {
        // Inisialisasi darah di awal game
        currentHealth = maxHealth;
    }

    // Fungsi publik yang akan dipanggil oleh script Player saat menyerang
    public void TakeDamage(int damage)
    {
        // Proteksi agar tidak memproses damage jika musuh sudah mati
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} terkena serangan! Sisa HP: {currentHealth}");

        // Cek apakah HP habis
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} telah kalah!");

        // 1. Jalankan perhitungan acak drop item
        CalculateEnemyDrop();

        // 2. Hancurkan objek musuh dari map
        // Catatan: Jika nanti ada animasi kalah, Destroy ini bisa dipanggil lewat Animation Event
        Destroy(gameObject);
    }

    private void CalculateEnemyDrop()
    {
        // Acak angka dari 0 sampai 99 (Total 100 kemungkinan)
        int roll = Random.Range(0, 100); 

        // RUMUS PROBABILITAS: 60 (Health) : 10 (Buff) : 30 (Nothing)
        
        if (roll < 60) // 60% Peluang (Angka 0 hingga 59)
        {
            if (healthDropPrefab != null)
            {
                Instantiate(healthDropPrefab, transform.position, Quaternion.identity);
                Debug.Log("Musuh drop: Es Cekek (Health Drop)");
            }
        }
        else if (roll < 70) // 10% Peluang (Angka 60 hingga 69)
        {
            // Jika masuk kategori Buff, kita acak lagi secara adil antara 3 buff yang tersedia
            int buffRoll = Random.Range(0, 3);
            GameObject chosenBuff = null;

            switch (buffRoll)
            {
                case 0: chosenBuff = hastePrefab; break;   // Sandal Swallow
                case 1: chosenBuff = berserkPrefab; break; // Kopi Bapak
                case 2: chosenBuff = aegisPrefab; break;   // Peci Haji
            }

            if (chosenBuff != null)
            {
                Instantiate(chosenBuff, transform.position, Quaternion.identity);
                Debug.Log($"Musuh drop Buff: {chosenBuff.name}");
            }
        }
        else // 30% Peluang Sisanya (Angka 70 hingga 99)
        {
            // ZONK / NOTHING: Tidak menjatuhkan item apa pun
            Debug.Log("Musuh tidak menjatuhkan item apa pun (Nothing).");
        }
    }
}
