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

        // Informar al MissionManager
        MissionManager manager = FindObjectOfType<MissionManager>();
        if (manager != null)
        {
            manager.AgregarProgreso(item, quantity);
        }
    }

    public Image GetItemIcon()
    {
        return itemIcon;
    }


    public bool AddQuantity(int amount)
    {
        if (currentItem == null || amount <= 0) return false;

        currentQuantity += amount;
        UpdateUI();

        // Informar al MissionManager
        MissionManager manager = FindObjectOfType<MissionManager>();
        if (manager != null)
        {
            manager.AgregarProgreso(currentItem, amount);
        }

        return true;
    }

    public bool RemoveQuantity(int amount)
    {
        if (currentItem == null || currentQuantity < amount) return false;

        currentQuantity -= amount;

        if (currentQuantity <= 0)
        {
            ClearSlot(); // ✅ Limpiar como cualquier otro ítem
        }
        else
        {
            UpdateUI();
        }

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
    public void UpdateQuantity(int newQuantity)
    {
        currentQuantity = newQuantity;
        quantityText.text = currentQuantity.ToString();
        // O cualquier otra lógica que necesites para actualizar la visualización
    }

    private void UpdateUI()
    {
        itemIcon.sprite = currentItem?.icon;
        itemIcon.enabled = currentItem != null;
        quantityText.text = currentQuantity > 1 ? currentQuantity.ToString() : "1";

        // Actualizar tag del icono (importante para las canecas)
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