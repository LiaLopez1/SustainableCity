using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public List<InventorySlot> slots;
    public ItemData itemPrueba;

    private void Start()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.ContainsItem(item))
            {
                int availableSpace = item.maxStack - slot.GetQuantity();
                if (availableSpace > 0)
                {
                    int toAdd = Mathf.Min(availableSpace, quantity);
                    slot.AddQuantity(toAdd);
                    quantity -= toAdd;
                    if (quantity <= 0) return true;
                }
            }
        }

        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.UpdateSlot(item, quantity);
                return true;
            }
        }

        Debug.LogWarning($"No hay espacio para {quantity} unidades de {item.itemName}");
        return false;
    }


    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        int remaining = quantity;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (!slots[i].IsEmpty() && slots[i].ContainsItem(item))
            {
                int available = slots[i].GetQuantity();
                int toRemove = Mathf.Min(available, remaining);

                if (slots[i].RemoveQuantity(toRemove))
                {
                    remaining -= toRemove;
                    if (remaining <= 0) return true;
                }
            }
        }

        Debug.LogWarning($"No se encontraron {quantity} unidades de {item.itemName} para remover");
        return false;
    }

    public bool TieneItem(ItemData item, int cantidad)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.GetItemData() == item)
            {
                total += slot.GetQuantity(); // ✅ correcto, así como está en tu script

                if (total >= cantidad) return true;
            }
        }
        return false;
    }


    /*private void Update()
    {
        // Solo para pruebas: presionar T agrega un ítem manualmente
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Asegúrate de tener una referencia al ítem de prueba
            AddItem(itemPrueba);
        }
    }*/
}