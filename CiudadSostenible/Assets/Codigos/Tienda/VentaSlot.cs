using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// Slot ÚNICO para vender con control por TAG y snapshot:
/// - Al primer drop: captura un snapshot de TODOS los InventorySlot que tengan el mismo tag.
/// - Reserva 1 unidad del slot de origen y queda en cantidad 1.
/// - Botón +: busca cualquier InventorySlot con el MISMO tag y stock > 0, reserva 1 y aumenta.
/// - Botón −: devuelve 1 al slot del que se tomó (manteniendo el estado original).
/// - Al cerrar panel o al reemplazar con otro ítem/tag: RESTAURA el inventario al snapshot inicial.
/// - Al vender: NO restaura (porque la venta se confirma) y se descarta el snapshot.
public class VentaSlot : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public Image itemIcon;
    public TextMeshProUGUI cantidadTexto;

    [Header("Referencias")]
    [Tooltip("UI de venta para validar vendibles/precio y gatillar cierres.")]
    public ZonaVentaUI zonaVentaUI;

    [Header("Inventario (opcional)")]
    [Tooltip("Si se deja vacío se auto-buscan todos los InventorySlot en escena.")]
    public List<InventorySlot> todosLosSlotsInventario = new List<InventorySlot>();

    // Estado del slot actual
    private ItemData currentItem;
    private string tagActual;
    private int cantidadReservada = 0;

    // Reservas por slot (para +/-)
    private readonly Dictionary<InventorySlot, int> reservasPorSlot = new Dictionary<InventorySlot, int>();

    // Snapshot del estado inicial (para RESTAURAR exacto al cerrar/reemplazar)
    private readonly Dictionary<InventorySlot, int> snapshotInicio = new Dictionary<InventorySlot, int>();
    private string snapshotTag = null;

    // ===== API pública usada por la UI =====
    public ItemData ObtenerItem() => currentItem;
    public int ObtenerCantidad() => cantidadReservada;
    public bool EstaVacio() => currentItem == null;

    void Awake()
    {
        if (todosLosSlotsInventario == null || todosLosSlotsInventario.Count == 0)
            todosLosSlotsInventario = new List<InventorySlot>(FindObjectsOfType<InventorySlot>(true));
    }

    /// Limpia el slot SIN devolver al inventario (para confirmar venta).
    public void LimpiarSinDevolver()
    {
        currentItem = null;
        tagActual = null;
        cantidadReservada = 0;
        reservasPorSlot.Clear();

        if (itemIcon != null) { itemIcon.sprite = null; itemIcon.enabled = false; }
        if (cantidadTexto != null) cantidadTexto.text = "0";
    }

    /// Restaura TODO el inventario al snapshot inicial y limpia.
    public void RestaurarYLimpiar()
    {
        RestaurarDesdeSnapshot();
        LimpiarSinDevolver();
    }

    /// Llamar al confirmar venta: descarta snapshot y reservas (no restaura).
    public void ConfirmarVentaFinalize()
    {
        snapshotInicio.Clear();
        snapshotTag = null;
        reservasPorSlot.Clear();
        LimpiarSinDevolver();
    }

    /// Botón +
    public bool TryIncrementar()
    {
        if (EstaVacio() || string.IsNullOrEmpty(tagActual)) return false;

        var slot = EncontrarSlotConTagDisponible(tagActual);
        if (slot == null) return false; // no hay más stock con ese tag

        slot.RemoveQuantity(1);
        SumarReserva(slot, 1);
        cantidadReservada++;
        ActualizarUI();
        return true;
    }

    /// Botón − (mínimo 1)
    public bool TryDecrementar()
    {
        if (EstaVacio()) return false;
        if (cantidadReservada <= 1) return false;

        var slot = ObtenerSlotConReservaMayor();
        if (slot == null) return false;

        slot.AddQuantity(1);
        RestarReserva(slot, 1);
        cantidadReservada--;
        ActualizarUI();
        return true;
    }

    // ===== Interno =====
    private void ConfigurarSlot(ItemData item, int cantidadInicial, InventorySlot origen)
    {
        currentItem = item;
        tagActual = item != null ? item.itemTag : null;
        cantidadReservada = Mathf.Max(1, cantidadInicial);

        reservasPorSlot.Clear();
        if (origen != null) SumarReserva(origen, cantidadInicial);

        if (itemIcon != null)
        {
            itemIcon.sprite = currentItem.icon;
            itemIcon.enabled = true;
        }
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (cantidadTexto != null)
            cantidadTexto.text = (cantidadReservada > 0) ? cantidadReservada.ToString() : "0";
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragItem = eventData.pointerDrag?.GetComponent<InventoryItemDragHandler>();
        if (dragItem == null) return;

        var slotOrigen = dragItem.GetComponentInParent<InventorySlot>();
        if (slotOrigen == null || slotOrigen.IsEmpty()) return;

        var itemDrop = slotOrigen.GetItemData();
        if (itemDrop == null) return;

        // Validación de vendibles
        if (zonaVentaUI != null && !zonaVentaUI.EsVendible(itemDrop))
        {
            Debug.Log($"El objeto '{itemDrop.name}' no se puede vender.");
            return;
        }

        // Si ya hay algo y es otro item/tag -> RESTAURAR inventario al snapshot previo
        bool mismoItem = itemDrop == currentItem;
        bool mismoTag = !string.IsNullOrEmpty(tagActual) && itemDrop.itemTag == tagActual;

        if (!EstaVacio() && !(mismoItem || mismoTag))
        {
            // Restaurar inventario al estado original del tag anterior
            RestaurarDesdeSnapshot();
            LimpiarSinDevolver();
        }

        // Si el slot está vacío (o acabamos de limpiar por reemplazo)
        if (EstaVacio())
        {
            // Capturar snapshot del inventario para el NUEVO tag
            CapturarSnapshot(itemDrop.itemTag);

            if (slotOrigen.GetQuantity() < 1) return;
            slotOrigen.RemoveQuantity(1);
            ConfigurarSlot(itemDrop, 1, slotOrigen);
            return;
        }

        // Mismo ítem/tag -> no acumular por drop (usa +/-)
        Debug.Log("No se acumula por drop. Usa los botones + / − para ajustar la cantidad.");
    }

    // ===== Snapshot =====
    private void CapturarSnapshot(string tag)
    {
        snapshotInicio.Clear();
        snapshotTag = tag ?? string.Empty;

        foreach (var slot in todosLosSlotsInventario)
        {
            if (slot == null || slot.IsEmpty()) continue;

            var data = slot.GetItemData();
            if (data == null) continue;
            if (data.itemTag != snapshotTag) continue;

            snapshotInicio[slot] = slot.GetQuantity();
        }
    }

    private void RestaurarDesdeSnapshot()
    {
        if (snapshotInicio.Count == 0) return;

        foreach (var kv in snapshotInicio)
        {
            var slot = kv.Key;
            if (slot == null) continue;

            int objetivo = kv.Value;
            int actual = slot.GetQuantity();
            int delta = objetivo - actual;

            if (delta > 0) slot.AddQuantity(delta);
            else if (delta < 0) slot.RemoveQuantity(-delta); // Seguridad por si algo más alteró el stock
        }

        snapshotInicio.Clear();
        snapshotTag = null;
        reservasPorSlot.Clear();
    }

    // ===== Utilidades de reserva =====
    private void SumarReserva(InventorySlot slot, int cant)
    {
        if (slot == null || cant <= 0) return;
        if (!reservasPorSlot.ContainsKey(slot)) reservasPorSlot[slot] = 0;
        reservasPorSlot[slot] += cant;
    }

    private void RestarReserva(InventorySlot slot, int cant)
    {
        if (slot == null || cant <= 0) return;
        if (!reservasPorSlot.ContainsKey(slot)) return;
        reservasPorSlot[slot] -= cant;
        if (reservasPorSlot[slot] <= 0) reservasPorSlot.Remove(slot);
    }

    private InventorySlot ObtenerSlotConReservaMayor()
    {
        InventorySlot best = null;
        int max = 0;
        foreach (var kv in reservasPorSlot)
        {
            if (kv.Value > max)
            {
                max = kv.Value;
                best = kv.Key;
            }
        }
        return best;
    }

    private InventorySlot EncontrarSlotConTagDisponible(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return null;
        InventorySlot candidato = null;
        int mejorCantidad = 0;

        foreach (var slot in todosLosSlotsInventario)
        {
            if (slot == null || slot.IsEmpty()) continue;

            var data = slot.GetItemData();
            if (data == null) continue;

            if (data.itemTag == tag && slot.GetQuantity() > 0)
            {
                int q = slot.GetQuantity();
                if (q > mejorCantidad)
                {
                    mejorCantidad = q;
                    candidato = slot;
                }
            }
        }
        return candidato;
    }
}
