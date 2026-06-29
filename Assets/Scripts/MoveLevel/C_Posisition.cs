using UnityEngine;

public class C_Posisition : MonoBehaviour
{
    void Start()
    {
        // Cek apakah ada catatan di memori?
        if (PindahLevel.memoriSpawn != "")
        {
            // Cari objek parkir di map dengan nama yang sesuai memori
            GameObject titikTarget = GameObject.Find(PindahLevel.memoriSpawn);
            
            // Kalau titiknya ketemu, langsung teleport Gojo ke sana
            if (titikTarget != null)
            {
                transform.position = titikTarget.transform.position;
            }
        }
    }
}