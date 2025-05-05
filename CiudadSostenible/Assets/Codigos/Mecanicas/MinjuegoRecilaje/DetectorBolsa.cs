using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DetectorBolsa : MonoBehaviour, IDropHandler
{
    [Header("Referencias UI")]
    public GameObject imagenBolsaAbierta;
    public List<ItemData> itemsPosibles;
    public Transform slotsContainer;
    public Button botonReinicio;

    [Header("Configuración")]
    public int cantidadSlots = 5;

    [Header("Mensaje de error")]
    public TextMeshProUGUI mensajeErrorTMP; // Asignar desde el Inspector
    public float duracionMensaje = 2f;

    private bool bolsaYaAbierta = false;
    private GameObject bolsaActual;

    void Start()
    {
        botonReinicio.onClick.AddListener(ReiniciarTotalmente);
        botonReinicio.gameObject.SetActive(false);
        if (mensajeErrorTMP != null)
            mensajeErrorTMP.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || !eventData.pointerDrag.CompareTag("BolsaBasura"))
            return;

        if (bolsaYaAbierta)
        {
            InventoryItemDragHandler handler = eventData.pointerDrag.GetComponent<InventoryItemDragHandler>();
            if (handler != null)
                handler.ReturnToInventory();

            if (mensajeErrorTMP != null)
                StartCoroutine(MostrarMensajeTemporal());

            return;
        }

        bolsaActual = eventData.pointerDrag;
        bolsaYaAbierta = true;

        // 🟢 Generar contenido antes de limpiar
        LimpiarSlotsCompletamente();
        GenerarElementosAleatorios();

        // 🧼 Luego limpiar inventario (como ya funciona normalmente)
        InventorySlot slotOriginal = bolsaActual.GetComponentInParent<InventorySlot>();
        if (slotOriginal != null)
        {
            slotOriginal.RemoveQuantity(1); // 🔥 Esto automáticamente limpia si llega a 0
        }

        imagenBolsaAbierta.SetActive(true);
        botonReinicio.gameObject.SetActive(false);
    }


    private void GenerarElementosAleatorios()
    {
        for (int i = 0; i < Mathf.Min(cantidadSlots, slotsContainer.childCount); i++)
        {
            if (itemsPosibles.Count == 0) break;

            GameObject itemIcon = new GameObject("ItemIcon");
            itemIcon.transform.SetParent(slotsContainer.GetChild(i));
            itemIcon.transform.localPosition = Vector3.zero;

            Image icono = itemIcon.AddComponent<Image>();
            itemIcon.AddComponent<DragItem>();
            itemIcon.AddComponent<CanvasGroup>();

            // Agregar detector de destrucción
            itemIcon.AddComponent<ObjetoEnBolsa>().detector = this;

            ItemData item = itemsPosibles[Random.Range(0, itemsPosibles.Count)];
            icono.sprite = item.icon;
            itemIcon.tag = item.itemTag;
        }
    }

    private void LimpiarSlotsCompletamente()
    {
        foreach (Transform slot in slotsContainer)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void RevisarSiBolsaEstaVacia()
    {
        foreach (Transform slot in slotsContainer)
        {
            if (slot.childCount > 0)
                return; // Todavía hay objetos
        }

        // Si llegó aquí, todos los slots están vacíos
        botonReinicio.gameObject.SetActive(true);
    }

    public void ReiniciarTotalmente()
    {
        LimpiarSlotsCompletamente();
        imagenBolsaAbierta.SetActive(false);
        botonReinicio.gameObject.SetActive(false);
        bolsaYaAbierta = false;
    }

    private IEnumerator MostrarMensajeTemporal()
    {
        mensajeErrorTMP.gameObject.SetActive(true);
        yield return new WaitForSeconds(duracionMensaje);
        mensajeErrorTMP.gameObject.SetActive(false);
    }
}
