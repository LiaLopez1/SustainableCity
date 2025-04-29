using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public List<InventorySlot> slots;

    private void Start()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }

    public void AddItem(ItemData item)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.HasItem(item))
            {
                slot.AddQuantity(1);
                return;
            }
        }

        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.UpdateSlot(item, 1);
                return;
            }
        }

        Debug.LogWarning("Inventario lleno");
    }

    public ItemData Plastic;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddItem(Plastic);
        }
    }
}