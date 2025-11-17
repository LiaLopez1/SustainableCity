using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Reflection;

/// VentaSlot con DEVOLUCIÓN EXACTA por unidad (pila LIFO) + AUTO-DESCUBRIMIENTO de inventario:
/// - No hace falta asignar InventorySlots por tienda; por defecto detecta todos los del jugador en escena.
/// - Opcionalmente, puedes limitar el alcance con 'inventarioRootOptional' (p.ej., el panel/objeto raíz del inventario).
public class VentaSlot : MonoBehaviour, IDropHandler
{
    [Header("UI")]
    public Image itemIcon;
    public TextMeshProUGUI cantidadTexto;

    [Header("Referencias")]
    public ZonaVentaUI zonaVentaUI;

    [Header("Descubrimiento de inventario")]
    [Tooltip("Si lo dejas vacío, se auto-buscan TODOS los InventorySlot de la escena (inventario global del jugador). " +
             "Si lo asignas, se buscarán SOLO dentro de este transform.")]
    public Transform inventarioRootOptional;

    // Estado del slot
    private ItemData currentItem;
    private string tagActual;

    // Registro por unidad (LIFO): de qué slot salió cada unidad reservada
    private readonly List<InventorySlot> pilaReservas = new List<InventorySlot>();

    // Cache local de slots encontrados
    private List<InventorySlot> _slotsCache = new List<InventorySlot>();
    private bool _cacheValida = false;

    // ===== API pública =====
    public ItemData ObtenerItem() => currentItem;
    public int ObtenerCantidad() => pilaReservas.Count;
    public bool EstaVacio() => currentItem == null;

    void OnEnable()
    {
        // Cada vez que se abra el panel, refrescamos la cache (por si algo cambió en la UI)
        RebuildInventarioSlots();
    }

    // ---------------- Botones ----------------

    public bool TryIncrementar()
    {
        if (EstaVacio() || string.IsNullOrEmpty(tagActual)) return false;
        if (!_cacheValida) RebuildInventarioSlots();

        var origen = EncontrarSlotConTagDisponible(tagActual);
        if (origen == null) return false;

        origen.RemoveQuantity(1);
        pilaReservas.Add(origen);
        ActualizarUI();
        return true;
    }

    public bool TryDecrementar()
    {
        if (EstaVacio()) return false;
        if (pilaReservas.Count <= 1) return false; // mínimo 1 visible en el slot

        var ultimoSlot = pilaReservas[pilaReservas.Count - 1];
        if (!TryDevolverUnaUnidad(ultimoSlot, currentItem))
        {
            if (!_cacheValida) RebuildInventarioSlots();
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

            // Asegura cache actualizada por si el usuario presiona + inmediatamente
            _cacheValida = false;
            RebuildInventarioSlots();
            return;
        }

        // Mismo ítem/tag -> no acumular por drop (usa +)
        Debug.Log("No se acumula por drop. Usa los botones + / − para ajustar la cantidad.");
    }

    // -------------- Cerrar / Vender --------------

    public void RestaurarYLimpiar()
    {
        RestaurarTodoAlInventario();
        LimpiarUI();
    }

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
        if (!_cacheValida) RebuildInventarioSlots();

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

    private bool TryDevolverUnaUnidad(InventorySlot slot, ItemData item)
    {
        if (slot == null || item == null) return false;

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

        var mSetItem2 = t.GetMethod("SetItem", BindingFlags.Instance | BindingFlags.Public, null, new System.Type[] { typeof(ItemData), typeof(int) }, null);
        if (mSetItem2 != null) { mSetItem2.Invoke(slot, new object[] { item, 0 }); LlamarUpdateUI(slot); return true; }

        var mSetItem1 = t.GetMethod("SetItem", BindingFlags.Instance | BindingFlags.Public, null, new System.Type[] { typeof(ItemData) }, null);
        if (mSetItem1 != null) { mSetItem1.Invoke(slot, new object[] { item }); LlamarUpdateUI(slot); return true; }

        var mAssign = t.GetMethod("AssignItem", BindingFlags.Instance | BindingFlags.Public);
        if (mAssign != null) { mAssign.Invoke(slot, new object[] { item }); LlamarUpdateUI(slot); return true; }

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

    // -------------- Descubrimiento de inventario --------------

    /// Reconstruye la lista de InventorySlots del jugador (global o bajo un root específico).
    public void RebuildInventarioSlots()
    {
        _slotsCache.Clear();

        if (inventarioRootOptional != null)
        {
            _slotsCache.AddRange(inventarioRootOptional.GetComponentsInChildren<InventorySlot>(true));
        }
        else
        {
            // Inventario global del jugador (toda la escena)
            _slotsCache.AddRange(FindObjectsOfType<InventorySlot>(true));
        }

        _cacheValida = true;
    }

    private InventorySlot EncontrarSlotConTagDisponible(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return null;

        InventorySlot candidato = null;
        int mejorCantidad = 0;

        // Busca slots con mismo tag y cantidad > 0
        for (int i = 0; i < _slotsCache.Count; i++)
        {
            var s = _slotsCache[i];
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

        for (int i = 0; i < _slotsCache.Count; i++)
        {
            var s = _slotsCache[i];
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
