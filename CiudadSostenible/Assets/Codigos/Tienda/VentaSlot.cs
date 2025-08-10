using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Reflection;

/// Slot ÚNICO para vender con control por TAG y snapshot ROBUSTO:
/// - Al primer drop: captura snapshot (slot, item, cantidad) de TODOS los InventorySlot con el mismo tag.
/// - + / −: reserva/libera unidades buscando por tag en todo el inventario.
/// - Cerrar/Reemplazar: RESTAURA exactamente el snapshot; si un slot quedó vacío, re-asigna el ItemData del snapshot antes de sumar.
/// - Vender: descarta snapshot y reservas (no restaura).
public class VentaSlot : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public Image itemIcon;
    public TextMeshProUGUI cantidadTexto;

    [Header("Referencias")]
    public ZonaVentaUI zonaVentaUI;

    [Header("Inventario (opcional)")]
    [Tooltip("Si se deja vacío, se auto-buscan todos los InventorySlot en escena.")]
    public List<InventorySlot> todosLosSlotsInventario = new List<InventorySlot>();

    // Estado del slot actual
    private ItemData currentItem;
    private string tagActual;
    private int cantidadReservada = 0;

    // Reservas (de dónde tomamos cuántas unidades, para el botón −)
    private readonly Dictionary<InventorySlot, int> reservasPorSlot = new Dictionary<InventorySlot, int>();

    // ===== Snapshot robusto =====
    private struct Snap
    {
        public InventorySlot slot;
        public ItemData item;
        public int cantidad;
    }
    private readonly List<Snap> snapshotInicio = new List<Snap>();
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

    /// Limpia el slot SIN devolver (venta confirmada lo usa luego de descartar snapshot).
    public void LimpiarSinDevolver()
    {
        currentItem = null;
        tagActual = null;
        cantidadReservada = 0;
        reservasPorSlot.Clear();

        if (itemIcon != null) { itemIcon.sprite = null; itemIcon.enabled = false; }
        if (cantidadTexto != null) cantidadTexto.text = "0";
    }

    /// Restaura EXACTO el snapshot (no se pierde nada) y limpia el slot.
    public void RestaurarYLimpiar()
    {
        RestaurarDesdeSnapshotRobusto();
        LimpiarSinDevolver();
    }

    /// Venta confirmada: descarta snapshot y reservas.
    public void ConfirmarVentaFinalize()
    {
        snapshotInicio.Clear();
        snapshotTag = null;
        reservasPorSlot.Clear();
        LimpiarSinDevolver();
    }

    // ===== Botones =====
    public bool TryIncrementar()
    {
        if (EstaVacio() || string.IsNullOrEmpty(tagActual)) return false;

        var slot = EncontrarSlotConTagDisponible(tagActual);
        if (slot == null) return false;

        slot.RemoveQuantity(1);
        SumarReserva(slot, 1);
        cantidadReservada++;
        ActualizarUI();
        return true;
    }

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

        if (zonaVentaUI != null && !zonaVentaUI.EsVendible(itemDrop))
        {
            Debug.Log($"El objeto '{itemDrop.name}' no se puede vender.");
            return;
        }

        bool mismoItem = itemDrop == currentItem;
        bool mismoTag = !string.IsNullOrEmpty(tagActual) && itemDrop.itemTag == tagActual;

        if (!EstaVacio() && !(mismoItem || mismoTag))
        {
            // Restaurar inventario del snapshot anterior ANTES de aceptar el nuevo
            RestaurarDesdeSnapshotRobusto();
            LimpiarSinDevolver();
        }

        if (EstaVacio())
        {
            CapturarSnapshotRobusto(itemDrop.itemTag);
            if (slotOrigen.GetQuantity() < 1) return;
            slotOrigen.RemoveQuantity(1);
            ConfigurarSlot(itemDrop, 1, slotOrigen);
            return;
        }

        Debug.Log("No se acumula por drop. Usa los botones + / − para ajustar la cantidad.");
    }

    // ===== Snapshot ROBUSTO =====
    private void CapturarSnapshotRobusto(string tag)
    {
        snapshotInicio.Clear();
        snapshotTag = tag ?? string.Empty;

        foreach (var slot in todosLosSlotsInventario)
        {
            if (slot == null || slot.IsEmpty()) continue;

            var data = slot.GetItemData();
            if (data == null) continue;
            if (data.itemTag != snapshotTag) continue;

            snapshotInicio.Add(new Snap { slot = slot, item = data, cantidad = slot.GetQuantity() });
        }
    }

    private void RestaurarDesdeSnapshotRobusto()
    {
        if (snapshotInicio.Count == 0) return;

        // Primero, asegurar que cada slot del snapshot tenga el ItemData correcto
        foreach (var s in snapshotInicio)
        {
            if (s.slot == null) continue;

            var actualItem = s.slot.GetItemData();
            if (actualItem == s.item)
                continue; // ya correcto

            if (s.slot.IsEmpty())
            {
                // El slot quedó vacío -> intentar re-asignar el item del snapshot
                if (!TryAsegurarItemEnSlot(s.slot, s.item))
                {
                    // No se pudo (slot no soporta asignación directa). Buscar un slot alterno.
                    var alterno = EncontrarSlotAlternoParaItem(s.item);
                    if (alterno != null && alterno != s.slot)
                    {
                        // Añadir ahí la cantidad objetivo; y marcar el original en 0
                        AjustarCantidadASnapshot(alterno, s.item, s.cantidad);
                        AjustarCantidadASnapshot(s.slot, s.item, 0);
                        continue;
                    }
                }
            }
            else
            {
                // El slot tiene otro item distinto al del snapshot -> fallback a alterno
                var alterno = EncontrarSlotAlternoParaItem(s.item);
                if (alterno != null && alterno != s.slot)
                {
                    AjustarCantidadASnapshot(alterno, s.item, s.cantidad);
                    AjustarCantidadASnapshot(s.slot, s.item, 0);
                    continue;
                }

                // Último recurso: reasignar por reflection el item correcto
                TryAsegurarItemEnSlot(s.slot, s.item);
            }
        }

        // Segundo, ajustar cantidades a lo que marcaba el snapshot
        foreach (var s in snapshotInicio)
        {
            if (s.slot == null) continue;
            AjustarCantidadASnapshot(s.slot, s.item, s.cantidad);
        }

        snapshotInicio.Clear();
        snapshotTag = null;
        reservasPorSlot.Clear();
    }

    /// Asegura que el slot apunte al ItemData dado (si está vacío u otro).
    private bool TryAsegurarItemEnSlot(InventorySlot slot, ItemData item)
    {
        if (slot == null || item == null) return false;

        // 1) Intentar algún método público típico: SetItem(item, qty) o Assign/Configure
        var t = slot.GetType();
        var mSetItem2 = t.GetMethod("SetItem", BindingFlags.Instance | BindingFlags.Public, null, new System.Type[] { typeof(ItemData), typeof(int) }, null);
        if (mSetItem2 != null) { mSetItem2.Invoke(slot, new object[] { item, 0 }); return true; }

        var mSetItem1 = t.GetMethod("SetItem", BindingFlags.Instance | BindingFlags.Public, null, new System.Type[] { typeof(ItemData) }, null);
        if (mSetItem1 != null) { mSetItem1.Invoke(slot, new object[] { item }); return true; }

        var mAssign = t.GetMethod("AssignItem", BindingFlags.Instance | BindingFlags.Public);
        if (mAssign != null) { mAssign.Invoke(slot, new object[] { item }); return true; }

        // 2) Reflection sobre campos privados comunes (currentItem / currentQuantity) + UpdateUI
        var fItem = t.GetField("currentItem", BindingFlags.Instance | BindingFlags.NonPublic);
        var fQty = t.GetField("currentQuantity", BindingFlags.Instance | BindingFlags.NonPublic);

        bool touched = false;
        if (fItem != null) { fItem.SetValue(slot, item); touched = true; }
        if (fQty != null) { fQty.SetValue(slot, 0); touched = true; }

        if (touched)
        {
            // Intentar refrescar UI
            var mUpdate = t.GetMethod("UpdateUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (mUpdate != null) mUpdate.Invoke(slot, null);
            return true;
        }

        return false;
    }

    /// Ajusta la cantidad del slot al objetivo del snapshot para ese item (creando si hace falta).
    private void AjustarCantidadASnapshot(InventorySlot slot, ItemData item, int objetivo)
    {
        if (slot == null) return;

        // Asegurar que el slot tenga el item correcto antes de tocar cantidades
        var actualItem = slot.GetItemData();
        if (actualItem == null)
        {
            if (!TryAsegurarItemEnSlot(slot, item)) return;
        }
        else if (actualItem != item)
        {
            // Si no coincide, intentar reasignar (o abandonar si el inventario no deja)
            if (!TryAsegurarItemEnSlot(slot, item)) return;
        }

        int actual = slot.GetQuantity();
        int delta = objetivo - actual;

        if (delta > 0) slot.AddQuantity(delta);
        else if (delta < 0) slot.RemoveQuantity(-delta);
    }

    /// Si un slot del snapshot no existe/acepta el item, buscamos otro slot candidato (vacío o con el mismo item).
    private InventorySlot EncontrarSlotAlternoParaItem(ItemData item)
    {
        InventorySlot vacio = null;
        foreach (var s in todosLosSlotsInventario)
        {
            if (s == null) continue;

            if (s.IsEmpty())
            {
                // Guardamos el primero vacío posible y seguimos por si encontramos uno con el mismo item
                if (vacio == null) vacio = s;
                continue;
            }

            var data = s.GetItemData();
            if (data == item) return s; // ideal: ya tiene el mismo ItemData
        }
        return vacio; // si no hay uno con el mismo item, devolvemos un vacío (lo asignaremos por reflection)
    }

    // ===== Utilidades de reserva para +/- =====
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
