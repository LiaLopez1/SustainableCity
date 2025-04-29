using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI quantityText;

    private ItemData currentItem;
    private int currentQuantity = 0;

    public void UpdateSlot(ItemData item, int quantity)
    {
        currentItem = item;
        currentQuantity = quantity;

        itemIcon.sprite = item.icon;
        itemIcon.enabled = true;
        quantityText.text = quantity.ToString();
    }

    public void AddQuantity(int quantity)
    {
        currentQuantity += quantity;
        quantityText.text = currentQuantity.ToString();
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentQuantity = 0;
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        quantityText.text = "";
    }

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public bool HasItem(ItemData item)
    {
        return currentItem == item;
    }
}