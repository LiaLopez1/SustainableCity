using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class CanecaReciclaje : MonoBehaviour, IDropHandler
{
    [System.Serializable]
    public class MaterialReciclable
    {
        public string tagMaterial; // "Papel", "Plastico", etc.
        public int cantidadActual = 0;
        public int cantidadMaxima = 10;
        public TextMeshProUGUI textoContador;
        public GameObject botonRecompensa;
        public ItemData recompensa;

        [HideInInspector] public bool estaCompleto;

    }

    [Header("Materiales")]
    public List<MaterialReciclable> materiales;

    [Header("Referencias")]
    public InventorySystem inventarioNormal;

    void Start()
    {
        InicializarContadores();
    }

    private void InicializarContadores()
    {
        foreach (var material in materiales)
        {
            material.textoContador.text = $"{material.cantidadActual}/{material.cantidadMaxima}";
            material.botonRecompensa.SetActive(false);
            
            // Configurar el evento del botón
            Button btn = material.botonRecompensa.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => ReclamarRecompensa(material));
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject itemArrastrado = eventData.pointerDrag;

        // 1. Intentar obtener el InventorySlot (para inventario normal)
        InventorySlot slotOrigen = itemArrastrado.GetComponentInParent<InventorySlot>();

        // 2. Procesar según el tipo de ítem
        if (slotOrigen != null)
        {
            ProcesarInventarioNormal(slotOrigen, itemArrastrado);
        }
        else
        {
            ProcesarBolsaBasura(itemArrastrado);
        }
    }

    private void ProcesarInventarioNormal(InventorySlot slot, GameObject item)
    {
        ItemData itemData = slot.GetItemData();
        int cantidadEnSlot = slot.GetQuantity();

        foreach (var material in materiales)
        {
            if (item.CompareTag(material.tagMaterial))
            {
                // Sumar TODAS las unidades del slot
                material.cantidadActual += cantidadEnSlot;
                material.estaCompleto = (material.cantidadActual >= material.cantidadMaxima);

                // Actualizar UI
                material.textoContador.text = $"{material.cantidadActual}/{material.cantidadMaxima}";
                if (material.botonRecompensa != null)
                    material.botonRecompensa.SetActive(material.estaCompleto);

                // Vaciar el slot
                slot.ClearSlot();
                return;
            }
        }

        // Si no coincide con ningún material, regresar al inventario
        InventoryItemDragHandler dragHandler = item.GetComponent<InventoryItemDragHandler>();
        if (dragHandler != null) dragHandler.ReturnToInventory();
    }

    private void ProcesarBolsaBasura(GameObject item)
    {
        foreach (var material in materiales)
        {
            if (item.CompareTag(material.tagMaterial))
            {
                material.cantidadActual++;
                material.estaCompleto = (material.cantidadActual >= material.cantidadMaxima);

                material.textoContador.text = $"{material.cantidadActual}/{material.cantidadMaxima}";
                if (material.botonRecompensa != null)
                    material.botonRecompensa.SetActive(material.estaCompleto);

                Destroy(item);
                return;
            }
        }
    }
    private void ReclamarRecompensa(MaterialReciclable material)
    {
        if (material.estaCompleto && material.recompensa != null)
        {
            // Añadir el ítem al inventario (1 unidad)
            inventarioNormal.AddItem(material.recompensa, 1);

            // Reiniciar contador
            material.cantidadActual = 0;
            material.estaCompleto = false;
            material.textoContador.text = $"{material.cantidadActual}/{material.cantidadMaxima}";
            material.botonRecompensa.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No hay recompensa definida o el material no está completo.");
        }
    }
}