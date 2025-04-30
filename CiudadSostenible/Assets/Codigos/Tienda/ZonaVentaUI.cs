using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ZonaVentaUI : MonoBehaviour
{
    public List<VentaSlot> slotsUI;
    public TextMeshProUGUI textoDinero;
    public InventorySystem inventario;

    private int dineroTotal = 0;

    public void VenderItems()
    {
        int totalVenta = 0;

        foreach (var slot in slotsUI)
        {
            if (!slot.EstaVacio())
            {
                var item = slot.ObtenerItem();
                int cantidad = slot.ObtenerCantidad();

                // ❌ NO volver a usar: inventario.RemoveItem(item, cantidad);

                totalVenta += cantidad * 10; // o el precio que definas
                slot.LimpiarSlot();
            }
        }

        dineroTotal += totalVenta;
        ActualizarUI();

        Debug.Log($"Venta completada por ${totalVenta}. Total: ${dineroTotal}");
    }


    private void ActualizarUI()
    {
        if (textoDinero != null)
            textoDinero.text = "$" + dineroTotal;
    }
}
