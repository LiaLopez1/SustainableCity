using UnityEngine;

[RequireComponent(typeof(TiendaInteraction))]
public class ShopContext : MonoBehaviour
{
    public TiendaComprasUI comprasUI;
    public ZonaVentaUI ventasUI;
    public GameObject panelComprasRoot;

    void Awake()
    {
        // Ya no inyectamos InventorySlots. VentaSlot se auto-descubre.
        if (ventasUI != null && ventasUI.ventaSlot != null)
            ventasUI.ventaSlot.zonaVentaUI = ventasUI; // por si acaso
    }

    public void OnOpenStore() { /* opcional */ }
    public void OnCloseStore()
    {
        if (ventasUI != null) ventasUI.CerrarPanel();
        if (panelComprasRoot != null) panelComprasRoot.SetActive(false);
    }
}
