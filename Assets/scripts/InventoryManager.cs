using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public int quantity;

    public InventoryItem(string name, int qty)
    {
        itemName = name;
        quantity = qty;
    }
}

public class InventoryManager : MonoBehaviour
{
    public List<InventoryItem> inventory = new List<InventoryItem>();

    public void AddItemToInventory(string itemName, int quantity = 1)
    {
        // Check if the item already exists in the inventory
        InventoryItem existingItem = inventory.Find(item => item.itemName == itemName);

        if (existingItem != null)
        {
            // If it exists, increase the quantity
            existingItem.quantity += quantity;
            Debug.Log(itemName + " miktar� " + existingItem.quantity + " olarak g�ncellendi.");
        }
        else
        {
            // If it doesn't exist, add a new item
            inventory.Add(new InventoryItem(itemName, quantity));
            Debug.Log(itemName + " envantere " + quantity + " miktar�yla eklendi.");
        }
    }

    public void RemoveItem(string itemName, int quantity = 1)
    {
        InventoryItem existingItem = inventory.Find(item => item.itemName == itemName);

        if (existingItem != null)
        {
            existingItem.quantity -= quantity;
            if (existingItem.quantity <= 0)
            {
                inventory.Remove(existingItem);
                Debug.Log(itemName + " envanterden kald�r�ld�.");
            }
            else
            {
                Debug.Log(itemName + " miktar� " + existingItem.quantity + " olarak azalt�ld�.");
            }
        }
        else
        {
            Debug.LogWarning(itemName + " envanterde bulunamad�.");
        }
    }

    public int GetItemQuantity(string itemName)
    {
        InventoryItem existingItem = inventory.Find(item => item.itemName == itemName);
        if (existingItem != null)
        {
            return existingItem.quantity;
        }
        return 0;
    }

    public void ListInventory()
    {
        Debug.Log("Envanter ��eri�i:");
        foreach (InventoryItem item in inventory)
        {
            Debug.Log(item.itemName + ": " + item.quantity);
        }
    }
}
