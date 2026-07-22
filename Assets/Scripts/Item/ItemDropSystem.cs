using System.Collections.Generic;
using UnityEngine;

public class ItemDropSystem : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public string itemName;
        public GameObject itemPrefab;
        public float dropChance; // Chance to drop this item (0 to 1)
    }

    public float overallDropChance = 70f;

    public List<DropItem> dropTable = new List<DropItem>();

    public void DropRandomItem(Vector3 dropPosition)
    {
        float dropRoll = Random.Range(0f, 100f);

        if (dropRoll > overallDropChance)
        {
            Debug.Log("No item dropped.");
            return;
        }

        if (dropTable == null || dropTable.Count == 0)
        {
            Debug.Log("No items available for dropping.");
            return;
        }

        float totalWeight = 0f;
        foreach (var item in dropTable)
        {
            if (item.itemPrefab != null && item.dropChance > 0)
            {
                totalWeight += item.dropChance;
            }
        }

        if (totalWeight <= 0f)
        {
            Debug.Log("No valid items to drop.");
            return;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        foreach (var item in dropTable)
        {
            if (item.itemPrefab == null && item.dropChance > 0) continue;

            currentSum += item.dropChance;

            if (randomValue <= currentSum)
            {
                Instantiate(item.itemPrefab, dropPosition, Quaternion.identity);
                Debug.Log($"Dropped item: {item.itemName}");
                return;
            }         
        }
    }
}