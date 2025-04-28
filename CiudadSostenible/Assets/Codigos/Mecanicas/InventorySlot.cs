using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image itemIcon;    // Arrastra el Image del icono aqu�
    public TextMeshProUGUI quantityText; // Arrastra el Text de cantidad aqu�
    private string itemName;  // Para rastrear el nombre del �tem

    // M�todo para actualizar el slot
    public void UpdateSlot(string newItemName, int quantity, Sprite icon)
    {
        itemName = newItemName;
        itemIcon.sprite = icon;
        quantityText.text = quantity.ToString();
        itemIcon.enabled = true; // Aseg�rate de que el icono sea visible
    }

    // M�todo para limpiar el slot
    public void ClearSlot()
    {
        itemName = null;
        itemIcon.sprite = null;
        quantityText.text = "";
        itemIcon.enabled = false;
    }

    // M�todo para verificar si el slot est� vac�o
    public bool IsEmpty()
    {
        return itemIcon.sprite == null; // O tambi�n: string.IsNullOrEmpty(itemName);
    }
}