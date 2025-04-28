using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class InventorySystem : MonoBehaviour
{
    public List<InventorySlot> slots; // Lista de slots (arrástralos desde el Inspector)

    void Start()
    {
        // Inicializa todos los slots como vacíos
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }

    public void AddItem(string itemName, int quantity, Sprite icon)
    {
        Debug.Log($"Intentando añadir: {itemName}"); // Verifica si el método se ejecuta

        if (icon == null) Debug.LogError("¡El icono es null!");
        else Debug.Log($"Icono asignado: {icon.name}");

        foreach (InventorySlot slot in slots)
        {
            if (slot == null)
            {
                Debug.LogError("¡Un slot en la lista es null!");
                continue;
            }

            if (slot.itemIcon == null) Debug.LogError("¡itemIcon en un slot es null!");
            if (slot.quantityText == null) Debug.LogError("¡quantityText en un slot es null!");

            if (slot.IsEmpty() || slot.itemIcon.sprite == icon)
            {
                slot.UpdateSlot(itemName, quantity, icon);
                Debug.Log($"Item añadido al slot: {slot.name}");
                return;
            }
        }
        Debug.LogWarning("¡Inventario lleno!");
    }
    //public void AddItem(string itemName, int quantity, Sprite icon)
    //{
    //    foreach (InventorySlot slot in slots)
    //    {
    //        // Si el slot está vacío o ya tiene el mismo item
    //        if (slot.IsEmpty() || slot.itemIcon.sprite == icon)
    //        {
    //            slot.UpdateSlot(itemName, quantity, icon);
    //            return;
    //        }
    //    }
    //    Debug.LogWarning("¡Inventario lleno!");
    //}


 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // Al presionar "T", añade plástico al inventario
        {
            Sprite plasticIcon = Resources.Load<Sprite>("Sprites/PlasticIcon");
            AddItem("Plástico", 1, plasticIcon);
        }
    }
}