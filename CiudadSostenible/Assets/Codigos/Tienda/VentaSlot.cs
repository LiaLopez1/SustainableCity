using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Reflection;

/// VentaSlot con DEVOLUCIÓN EXACTA por unidad (pila LIFO):
/// - Drop: toma 1 unidad y registra su slot de origen en una pila.
/// - + : busca un slot con el mismo tag, quita 1 y lo apila.
/// - − : saca de la pila y devuelve 1 al slot exacto; si está vacío, re-asigna ItemData; si falla, usa fallback.
/// - Cerrar: devuelve TODAS las unidades de la pila una por una (sin “comerse” nada).
/// - Vender: limpia sin devolver (venta confirmada).
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

    // Estado del slot
    private ItemData currentItem;
    private string tagActual;

    // Registro por UNIDAD: cada entrada es el slot del que salió esa unidad
    private readonly List<InventorySlot> pilaReservas = new List<InventorySlot>(); // LIFO

    // ===== API pública =====
    public ItemData ObtenerItem() => currentItem;
    public int ObtenerCantidad() => pilaReservas.Count;
    public bool EstaVacio() => currentItem == null;

    void Awake()
    {
        if (todosLosSlotsInventario == null || todosLosSlotsInventario.Count == 0)
            todosLosSlotsInventario = new List<InventorySlot>(FindObjectsOfType<InventorySlot>(true));
    }

    // ---------------- Botones ----------------

    public bool TryIncrementar()
    {
        if (EstaVacio() || string.IsNullOrEmpty(tagActual)) return false;

        var origen = EncontrarSlotConTagDisponible(tagActual);
        if (origen == null) return false;

        // Toma 1 del origen y registra EXACTAMENTE de dónde salió
        origen.RemoveQuantity(1);
        pilaReservas.Add(origen);
        ActualizarUI();
        return true;
    }

    public bool TryDecrementar()
    {
        if (EstaVacio()) return false;
        if (pilaReservas.Count <= 1) return false; // mínimo 1 en el slot

        var ultimoSlot = pilaReservas[pilaReservas.Count - 1];
        if (!TryDevolverUnaUnidad(ultimoSlot, currentItem))
        {
            // Fallback si el slot original ya no acepta
            var alterno = EncontrarSlotAlternoParaItem(currentItem);
            if (alterno == null || !TryDevolverUnaUnidad(alterno, currentItem))
            {
                Debug.LogWarning("No se pudo devolver una unidad; se mantiene reservada para no perderla.");
                return false;
            }
        }

        pilaReservas.RemoveAt(pilaReservas.Count - 1);
        ActualizarUI();
        return true;
    }

    // -------------- Flujo de drop --------------

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

        bool mismoItem = itemDrop == currentItem;
        bool mismoTag = !string.IsNullOrEmpty(tagActual) && itemDrop.itemTag == tagActual;

        // Si hay algo distinto, devolver TODO antes de aceptar el nuevo
        if (!EstaVacio() && !(mismoItem || mismoTag))
        {
            RestaurarTodoAlInventario();
            LimpiarUI();
        }

        if (EstaVacio())
        {
            if (slotOrigen.GetQuantity() < 1) return;

            // Primer unidad: toma 1 y apila su origen
            slotOrigen.RemoveQuantity(1);
            currentItem = itemDrop;
            tagActual = currentItem.itemTag;
            pilaReservas.Clear();
            pilaReservas.Add(slotOrigen);

            if (itemIcon != null) { itemIcon.sprite = currentItem.icon; itemIcon.enabled = true; }
            ActualizarUI();
            return;
        }

        // Mismo ítem/tag -> no acumular por drop (usa +)
        Debug.Log("No se acumula por drop. Usa los botones + / − para ajustar la cantidad.");
    }

    // -------------- Cerrar / Vender --------------

    /// Llamar al cerrar el panel (ZonaVentaUI.CerrarPanel): devuelve todo y limpia.
    public void RestaurarYLimpiar()
    {
        RestaurarTodoAlInventario();
        LimpiarUI();
    }

    /// Llamar al vender: no se devuelve (ya se vendió), solo limpiar.
    public void ConfirmarVentaFinalize()
    {
        pilaReservas.Clear();
        LimpiarUI();
    }

    // -------------- Internos --------------

    private void ActualizarUI()
    {
        if (cantidadTexto != null)
            cantidadTexto.text = (pilaReservas.Count > 0) ? pilaReservas.Count.ToString() : "0";
    }

    private void LimpiarUI()
    {
        currentItem = null;
        tagActual = null;

        if (itemIcon != null) { itemIcon.sprite = null; itemIcon.enabled = false; }
        if (cantidadTexto != null) cantidadTexto.text = "0";
    }

    private void RestaurarTodoAlInventario()
    {
        if (pilaReservas.Count == 0 || currentItem == null) { pilaReservas.Clear(); return; }

        // Devolver una por una en orden inverso (LIFO)
        for (int i = pilaReservas.Count - 1; i >= 0; i--)
        {
            var slot = pilaReservas[i];
            if (!TryDevolverUnaUnidad(slot, currentItem))
            {
                var alterno = EncontrarSlotAlternoParaItem(currentItem);
                if (alterno != null) TryDevolverUnaUnidad(alterno, currentItem);
                else Debug.LogWarning("No se encontró slot para devolver una unidad; se omite para no duplicar.");
            }
        }
        pilaReservas.Clear();
    }

    // -------------- Utilidades de devolución / fallback --------------

    private bool TryDevolverUnaUnidad(InventorySlot slot, ItemData item)
    {
        if (slot == null || item == null) return false;

        // Si el slot está vacío o tiene otro item, intenta re-asignar el item correcto.
        var actual = slot.GetItemData();
        if (actual == null || actual != item)
        {
            if (!TryAsegurarItemEnSlot(slot, item))
                return false;
        }

        slot.AddQuantity(1);
        return true;
    }

    private bool TryAsegurarItemEnSlot(InventorySlot slot, ItemData item)
    {
        var t = slot.GetType();

        // Métodos públicos comunes
        var mSetItem2 = t.GetMethod("SetItem", BindingFlags.Instance | BindingFlags.Public, null, new System.Type[] { typeof(ItemData), typeof(int) }, null);
        if (mSetItem2 != null) { mSetItem2.Invoke(slot, new object[] { item, 0 }); LlamarUpdateUI(slot); return true; }

        var mSetItem1 = t.GetMethod("SetItem", BindingFlags.Instance | BindingFlags.Public, null, new System.Type[] { typeof(ItemData) }, null);
        if (mSetItem1 != null) { mSetItem1.Invoke(slot, new object[] { item }); LlamarUpdateUI(slot); return true; }

        var mAssign = t.GetMethod("AssignItem", BindingFlags.Instance | BindingFlags.Public);
        if (mAssign != null) { mAssign.Invoke(slot, new object[] { item }); LlamarUpdateUI(slot); return true; }

        // Campos privados comunes
        var fItem = t.GetField("currentItem", BindingFlags.Instance | BindingFlags.NonPublic);
        var fQty = t.GetField("currentQuantity", BindingFlags.Instance | BindingFlags.NonPublic);

        bool touched = false;
        if (fItem != null) { fItem.SetValue(slot, item); touched = true; }
        if (fQty != null) { fQty.SetValue(slot, 0); touched = true; }

        if (touched) { LlamarUpdateUI(slot); return true; }
        return false;
    }

    private void LlamarUpdateUI(InventorySlot slot)
    {
        var mUpdate = slot.GetType().GetMethod("UpdateUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (mUpdate != null) mUpdate.Invoke(slot, null);
    }

    private InventorySlot EncontrarSlotConTagDisponible(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return null;
        InventorySlot candidato = null;
        int mejorCantidad = 0;

        foreach (var s in todosLosSlotsInventario)
        {
            if (s == null || s.IsEmpty()) continue;
            var data = s.GetItemData();
            if (data == null || data.itemTag != tag) continue;

            int q = s.GetQuantity();
            if (q > 0 && q > mejorCantidad)
            {
                mejorCantidad = q;
                candidato = s;
            }
        }
        return candidato;
    }

    private InventorySlot EncontrarSlotAlternoParaItem(ItemData item)
    {
        InventorySlot vacio = null;
        foreach (var s in todosLosSlotsInventario)
        {
            if (s == null) continue;

            if (s.IsEmpty())
            {
                if (vacio == null) vacio = s;
                continue;
            }

            var data = s.GetItemData();
            if (data == item) return s;
        }
        return vacio;
    }
}
