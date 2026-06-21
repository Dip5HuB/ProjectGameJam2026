using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollectible : MonoBehaviour
{
    public enum ItemCategory { Health, Haste, Berserk, Aegis }
    
    [Header("Item Config")]
    public ItemCategory itemCategory;
    public float durationValue = 6f; // Durasi aktif buff (misal 6 detik)
    public int healValue = 25;       // Jumlah pemulihan HP untuk Es Cekek

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Pastikan objek yang menabrak memiliki Tag "Player"
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            
            if (player != null)
            {
                if (itemCategory == ItemCategory.Health)
                {
                    player.ApplyHealthDrop(healValue);
                }
                else
                {
                    // Konversi kategori item menjadi tipe buff di PlayerController
                    PlayerController.BuffType targetBuff = itemCategory switch
                    {
                        ItemCategory.Haste => PlayerController.BuffType.Haste,
                        ItemCategory.Berserk => PlayerController.BuffType.Berserk,
                        ItemCategory.Aegis => PlayerController.BuffType.Aegis,
                        _ => PlayerController.BuffType.None
                    };

                    // Kirim tipe buff ke skrip utama player
                    player.ApplyBuff(targetBuff, durationValue);
                }

                // Hancurkan objek item di tanah setelah dikonsumsi
                Destroy(gameObject);
            }
        }
    }
}
