using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;

    [Header("Datos del Ítem")]
    [SerializeField] private ItemData currentItem;
    [SerializeField] private int currentQuantity = 0;

    // Métodos públicos para acceso controlado
    public ItemData GetItemData() => currentItem;
    public int GetQuantity() => currentQuantity;
    public bool IsEmpty() => currentItem == null;
    public bool ContainsItem(ItemData item) => currentItem == item;

    public void UpdateSlot(ItemData item, int quantity)
    {
        currentItem = item;
        currentQuantity = quantity;
        UpdateUI();
    }

    public bool AddQuantity(int amount)
    {
        if (currentItem == null || amount <= 0) return false;

        currentQuantity += amount;
        UpdateUI();
        return true;
    }

    public bool RemoveQuantity(int amount)
    {
        if (currentItem == null || currentQuantity < amount) return false;

        currentQuantity -= amount;
        if (currentQuantity <= 0)
            ClearSlot();
        else
            UpdateUI();
        return true;
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentQuantity = 0;
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        quantityText.text = "";
    }

    private void UpdateUI()
    {
        itemIcon.sprite = currentItem?.icon;
        itemIcon.enabled = currentItem != null;
        quantityText.text = currentQuantity > 1 ? currentQuantity.ToString() : "";

        // Actualizar tag si hay ítem
        if (currentItem != null && !string.IsNullOrEmpty(currentItem.itemTag))
        {
            itemIcon.gameObject.tag = currentItem.itemTag;
        }
        else
        {
            itemIcon.gameObject.tag = "Untagged";
        }
    }
}