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

                // Aquí ya no removemos del inventario porque se hizo en el drop

                totalVenta += cantidad * 10; // Valor por unidad (puedes hacerlo dinámico)
                slot.LimpiarSlot();
            }
        }

        // Aquí usamos el sistema de economía global
        EconomiaJugador.Instance.AgregarDinero(totalVenta);

        Debug.Log($"Venta completada por ${totalVenta}. Dinero total actual: ${EconomiaJugador.Instance.ObtenerDinero()}");
    }

    private void ActualizarUI()
    {
        if (textoDinero != null)
            textoDinero.text = "$" + dineroTotal;
    }
}
