using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class VentaSlot : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public Image itemIcon;
    public TextMeshProUGUI cantidadTexto;

    [Header("Datos del ítem")]
    private ItemData currentItem;
    private int cantidad;

    public void ConfigurarSlot(ItemData item, int cantidadInicial)
    {
        currentItem = item;
        cantidad = cantidadInicial;

        if (itemIcon != null)
        {
            itemIcon.sprite = currentItem.icon;
            itemIcon.enabled = true;
        }

        ActualizarUI();
    }


    public ItemData ObtenerItem() => currentItem;
    public int ObtenerCantidad() => cantidad;
    public bool EstaVacio() => currentItem == null;

    public void LimpiarSlot()
    {
        currentItem = null;
        cantidad = 0;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false; // Oculta solo el ícono del ítem, no el fondo blanco
        }

        if (cantidadTexto != null)
            cantidadTexto.text = "0"; // Siempre mostrar 0 cuando está vacío
    }


    private void ActualizarUI()
    {
        cantidadTexto.text = cantidad > 1 ? cantidad.ToString() : "1";
    }


    public void OnDrop(PointerEventData eventData)
    {
        var dragItem = eventData.pointerDrag?.GetComponent<InventoryItemDragHandler>();
        if (dragItem == null) return;

        var slotOrigen = dragItem.GetComponentInParent<InventorySlot>();
        if (slotOrigen == null || slotOrigen.IsEmpty()) return;

        // Justo después de obtener el item desde el slotOrigen:
        var item = slotOrigen.GetItemData();

        // Nueva línea: descontar 1 unidad del inventario real
        slotOrigen.RemoveQuantity(1); // Ya existe esta función en InventorySlot.cs


        if (EstaVacio())
        {
            ConfigurarSlot(item, 1); // ✅ Ahora sí funciona después de limpiar
        }
        else if (item == currentItem)
        {
            cantidad++;
            ActualizarUI();
        }
    }

}
