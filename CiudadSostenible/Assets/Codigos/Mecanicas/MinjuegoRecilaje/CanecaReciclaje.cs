using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

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
        public string tipoMaterial;

        [HideInInspector] public bool estaCompleto;
        [HideInInspector] public bool bloqueoEntrada = false; // NUEVO: bloqueo al llenarse
    }

    [Header("Materiales")]
    public List<MaterialReciclable> materiales;

    [Header("Referencias")]
    public InventorySystem inventarioNormal;

    [Header("Mensaje si inventario está lleno")]
    public TextMeshProUGUI mensajeInventarioLleno;
    public float duracionMensaje = 1f;

    void Start()
    {
        InicializarContadores();
    }

    private void InicializarContadores()
    {
        foreach (var material in materiales)
        {
            material.textoContador.text = $"{material.tipoMaterial}: {material.cantidadActual}/{material.cantidadMaxima}";
            material.textoContador.gameObject.SetActive(true);
            material.botonRecompensa.SetActive(false);
            material.bloqueoEntrada = false;

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
        InventorySlot slotOrigen = itemArrastrado.GetComponentInParent<InventorySlot>();

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
        if (itemData == null) return;

        int cantidadEnSlot = slot.GetQuantity();

        foreach (var material in materiales)
        {
            if (item.CompareTag(material.tagMaterial))
            {
                if (material.bloqueoEntrada) return;

                int espacioDisponible = material.cantidadMaxima - material.cantidadActual;
                int cantidadATomar = Mathf.Min(espacioDisponible, cantidadEnSlot);

                // Si no hay espacio suficiente, no hacemos nada
                if (cantidadATomar <= 0) return;

                material.cantidadActual += cantidadATomar;
                material.estaCompleto = (material.cantidadActual >= material.cantidadMaxima);

                material.textoContador.text = $"{material.tipoMaterial}: {material.cantidadActual}/{material.cantidadMaxima}";
                if (material.botonRecompensa != null)
                    material.botonRecompensa.SetActive(material.estaCompleto);

                if (material.estaCompleto)
                {
                    material.textoContador.gameObject.SetActive(false);
                    material.bloqueoEntrada = true;
                }

                // Actualizamos el slot del inventario
                if (cantidadATomar == cantidadEnSlot)
                {
                    // Si tomamos todo, limpiamos el slot
                    slot.ClearSlot();
                }
                else
                {
                    // Si tomamos solo parte, actualizamos la cantidad
                    slot.UpdateQuantity(cantidadEnSlot - cantidadATomar);
                }

                return;
            }
        }

        // Si no coincide con ningún material, devolvemos el item
        InventoryItemDragHandler dragHandler = item.GetComponent<InventoryItemDragHandler>();
        if (dragHandler != null) dragHandler.ReturnToInventory();
    }

    private void ProcesarBolsaBasura(GameObject item)
    {
        foreach (var material in materiales)
        {
            if (item.CompareTag(material.tagMaterial))
            {
                if (material.bloqueoEntrada) return;

                material.cantidadActual++;
                material.estaCompleto = (material.cantidadActual >= material.cantidadMaxima);

                material.textoContador.text = $"{material.tipoMaterial}: {material.cantidadActual}/{material.cantidadMaxima}";
                if (material.botonRecompensa != null)
                    material.botonRecompensa.SetActive(material.estaCompleto);

                if (material.estaCompleto)
                {
                    material.textoContador.gameObject.SetActive(false);
                    material.bloqueoEntrada = true;
                }

                Destroy(item);
                return;
            }
        }
    }

    private void ReclamarRecompensa(MaterialReciclable material)
    {
        if (!material.estaCompleto || material.recompensa == null)
            return;

        if (inventarioNormal == null)
            return;

        bool agregado = inventarioNormal.AddItem(material.recompensa, 1);

        if (agregado)
        {
            material.cantidadActual = 0;
            material.estaCompleto = false;
            material.bloqueoEntrada = false;
            material.textoContador.text = $"{material.tipoMaterial}: {material.cantidadActual}/{material.cantidadMaxima}";
            material.textoContador.gameObject.SetActive(true);
            material.botonRecompensa.SetActive(false);
        }
        else
        {
            if (mensajeInventarioLleno != null)
            {
                mensajeInventarioLleno.gameObject.SetActive(true);
                StartCoroutine(DesactivarMensajeTMP());
            }
        }
    }

    private IEnumerator DesactivarMensajeTMP()
    {
        yield return new WaitForSeconds(duracionMensaje);
        if (mensajeInventarioLleno != null)
        {
            mensajeInventarioLleno.gameObject.SetActive(false);
        }
    }
}
