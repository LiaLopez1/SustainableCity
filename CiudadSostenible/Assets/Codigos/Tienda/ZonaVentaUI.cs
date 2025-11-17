using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class ItemPrecio
{
    public ItemData item;
    public int precio;
}

public class ZonaVentaUI : MonoBehaviour
{
    [Header("Referencias")]
    public VentaSlot ventaSlot;
    public TextMeshProUGUI textoDinero;

    [Header("Panel de venta")]
    [Tooltip("Arrastra aquí el GameObject del panel para poder cerrarlo al pulsar el botón Cerrar.")]
    public GameObject panelVenta;

    [Header("UI Cantidad / Precio")]
    public Button botonMas;
    public Button botonMenos;
    public TextMeshProUGUI textoCantidad;
    public TextMeshProUGUI textoPrecioUnitario;
    public TextMeshProUGUI textoPrecioTotal;

    [Header("Acciones")]
    public Button botonVender;
    public Button botonCerrar;

    [Header("Precios (por tag - Fallback)")]
    public int precioDefault = 10;
    public Dictionary<string, int> preciosPorTag = new Dictionary<string, int>
    {
        { "oro", 150 },
        { "plata", 80 },
        { "madera", 15 },
        { "pocion", 40 },
        { "comida", 12 },
    };

    [Header("Listas de control")]
    public List<ItemPrecio> itemsVendibles = new List<ItemPrecio>();
    public List<ItemData> itemsNoVendibles = new List<ItemData>();

    // Estado
    private ItemData itemActual;
    private int precioUnitario;

    private void RebindButtons()
    {
        if (botonMas != null)
        {
            botonMas.onClick.RemoveAllListeners();
            botonMas.onClick.AddListener(OnClickMas);
        }
        if (botonMenos != null)
        {
            botonMenos.onClick.RemoveAllListeners();
            botonMenos.onClick.AddListener(OnClickMenos);
        }
        if (botonVender != null)
        {
            botonVender.onClick.RemoveAllListeners();
            botonVender.onClick.AddListener(VenderActual);
        }
        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(CerrarPanel);
        }
    }

    void Awake()
    {
        if (ventaSlot == null) ventaSlot = GetComponentInChildren<VentaSlot>(true);
        if (ventaSlot != null) ventaSlot.zonaVentaUI = this;

        RebindButtons();           // <-- usa esto en vez de AddListener “en crudo”
        RefrescarDesdeSlot();
    }

    void Start()
    {
        // Primer pintado del dinero, pero sin romper si la economía aún no está lista
        ActualizarDineroUI();
    }

    void Update()
    {
        if (ventaSlot == null) return;

        var nuevoItem = ventaSlot.ObtenerItem();
        var cant = ventaSlot.ObtenerCantidad();

        if (nuevoItem != itemActual)
            RefrescarDesdeSlot();
        else
            ActualizarTotales(cant);
    }

    private void RefrescarDesdeSlot()
    {
        itemActual = ventaSlot != null ? ventaSlot.ObtenerItem() : null;
        int cantidad = ventaSlot != null ? ventaSlot.ObtenerCantidad() : 0;

        precioUnitario = PrecioUnitarioPara(itemActual);

        bool hayItem = itemActual != null;

        if (textoCantidad != null) textoCantidad.text = hayItem ? Mathf.Max(1, cantidad).ToString() : "0";
        if (textoPrecioUnitario != null) textoPrecioUnitario.text = "$" + (hayItem ? precioUnitario : 0);
        if (textoPrecioTotal != null) textoPrecioTotal.text = "$" + (hayItem ? (precioUnitario * Mathf.Max(1, cantidad)) : 0);

        if (botonMas != null) botonMas.interactable = hayItem;
        if (botonMenos != null) botonMenos.interactable = hayItem && cantidad > 1;
        if (botonVender != null) botonVender.interactable = hayItem;
    }

    private void ActualizarTotales(int cantidad)
    {
        bool hayItem = itemActual != null;
        if (textoCantidad != null) textoCantidad.text = hayItem ? Mathf.Max(1, cantidad).ToString() : "0";
        if (textoPrecioTotal != null) textoPrecioTotal.text = "$" + (hayItem ? (precioUnitario * Mathf.Max(1, cantidad)) : 0);

        if (botonMenos != null) botonMenos.interactable = hayItem && cantidad > 1;
    }

    public bool EsVendible(ItemData item)
    {
        if (item == null) return false;

        if (itemsVendibles != null && itemsVendibles.Count > 0)
        {
            foreach (var ip in itemsVendibles)
                if (ip != null && ip.item == item)
                    return true;
            return false;
        }

        return itemsNoVendibles == null || !itemsNoVendibles.Contains(item);
    }

    public int PrecioUnitarioPara(ItemData item)
    {
        if (item == null) return 0;

        if (itemsVendibles != null && itemsVendibles.Count > 0)
        {
            foreach (var ip in itemsVendibles)
                if (ip != null && ip.item == item)
                    return Mathf.Max(0, ip.precio);
            return 0;
        }

        var tag = item.itemTag;
        if (!string.IsNullOrEmpty(tag) && preciosPorTag.TryGetValue(tag.ToLowerInvariant(), out int p))
            return Mathf.Max(0, p);

        return Mathf.Max(0, precioDefault);
    }

    public void OnClickMas()
    {
        if (ventaSlot == null || ventaSlot.EstaVacio()) return;
        if (ventaSlot.TryIncrementar())
        {
            int cantidad = ventaSlot.ObtenerCantidad();
            ActualizarTotales(cantidad);
        }
    }

    public void OnClickMenos()
    {
        if (ventaSlot == null || ventaSlot.EstaVacio()) return;
        if (ventaSlot.TryDecrementar())
        {
            int cantidad = ventaSlot.ObtenerCantidad();
            ActualizarTotales(cantidad);
        }
    }

    public void VenderActual()
    {
        if (ventaSlot == null || ventaSlot.EstaVacio()) return;

        int cantidad = Mathf.Max(1, ventaSlot.ObtenerCantidad());
        int total = cantidad * precioUnitario;

        EconomiaJugador.Instance.AgregarDinero(total);
        Debug.Log($"Venta: {cantidad} x {itemActual.name} @ ${precioUnitario} = ${total}. Dinero: ${EconomiaJugador.Instance.ObtenerDinero()}");

        ventaSlot.ConfirmarVentaFinalize();
        RefrescarDesdeSlot();
        ActualizarDineroUI();

        // Cierra panel después de vender si está asignado
        if (panelVenta != null)
            panelVenta.SetActive(false);
    }

    public void CerrarPanel()
    {
        if (ventaSlot != null && !ventaSlot.EstaVacio())
            ventaSlot.RestaurarYLimpiar();

        RefrescarDesdeSlot();

        // 🔹 Cerrar visualmente el panel
        if (panelVenta != null)
            panelVenta.SetActive(false);
    }

    private void ActualizarDineroUI()
    {
        if (textoDinero == null) return;

        var econ = EconomiaJugador.Instance; // podría ser null si no está inicializado aún
        int monto = 0;
        if (econ != null)
        {
            // Protegemos también ObtenerDinero() por si lanza algo raro
            try { monto = econ.ObtenerDinero(); } catch { monto = 0; }
        }

        textoDinero.text = "$" + monto;
    }
}
