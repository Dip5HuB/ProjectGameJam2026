using UnityEngine;

public class ItemDropSystem : MonoBehaviour
{
    [Header("Drop Prefabs Item")]
    [SerializeField] private GameObject healthDropPrefab;  // Es Cekek
    [SerializeField] private GameObject hastePrefab;       // Sandal Swallow
    [SerializeField] private GameObject berserkPrefab;     // Kopi Bapak
    [SerializeField] private GameObject aegisPrefab;       // Peci Haji

    // Fungsi publik yang menerima parameter posisi spawn (bisa dipanggil dari mana saja)
    public void DropRandomItem(Vector3 spawnPosition)
    {
        // Acak angka dari 0 sampai 99 (Total 100 kemungkinan)
        int roll = Random.Range(0, 100); 

        // RUMUS PROBABILITAS: 60 (Health) : 10 (Buff) : 30 (Nothing)
        if (roll < 60) // 60% Peluang (Angka 0 hingga 59)
        {
            if (healthDropPrefab != null)
            {
                Instantiate(healthDropPrefab, spawnPosition, Quaternion.identity);
                Debug.Log("Drop System: Mengeluarkan Es Cekek!");
            }
        }
        else if (roll < 70) // 10% Peluang (Angka 60 hingga 69)
        {
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
                Instantiate(chosenBuff, spawnPosition, Quaternion.identity);
                Debug.Log($"Drop System: Mengeluarkan Buff {chosenBuff.name}!");
            }
        }
        else // 30% Peluang Sisanya (Angka 70 hingga 99)
        {
            Debug.Log("Drop System: Tidak ada item yang keluar (Nothing).");
        }
    }
}