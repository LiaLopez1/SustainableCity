using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image itemIcon;    // Arrastra el Image del icono aquí
    public TextMeshProUGUI quantityText; // Arrastra el Text de cantidad aquí
    private string itemName;  // Para rastrear el nombre del ítem

    // Método para actualizar el slot
    public void UpdateSlot(string newItemName, int quantity, Sprite icon)
    {
        itemName = newItemName;
        itemIcon.sprite = icon;
        quantityText.text = quantity.ToString();
        itemIcon.enabled = true; // Asegúrate de que el icono sea visible
    }

    // Método para limpiar el slot
    public void ClearSlot()
    {
        itemName = null;
        itemIcon.sprite = null;
        quantityText.text = "";
        itemIcon.enabled = false;
    }

    // Método para verificar si el slot está vacío
    public bool IsEmpty()
    {
        return itemIcon.sprite == null; // O también: string.IsNullOrEmpty(itemName);
    }
}