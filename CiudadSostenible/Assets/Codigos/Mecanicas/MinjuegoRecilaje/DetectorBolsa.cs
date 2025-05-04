using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DetectorBolsa : MonoBehaviour, IDropHandler
{
    [Header("Referencias UI")]
    public GameObject imagenBolsaAbierta;
    public List<ItemData> itemsPosibles;
    public Transform slotsContainer;
    public Button botonReinicio;

    [Header("Configuración")]
    public int cantidadSlots = 5;

    void Start()
    {
        botonReinicio.onClick.AddListener(ReiniciarTotalmente);
        botonReinicio.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || !eventData.pointerDrag.CompareTag("BolsaBasura"))
            return;

        // 1. Limpieza total antes de generar
        LimpiarSlotsCompletamente();

        // 2. Generar nuevos ítems
        GenerarElementosAleatorios();

        // 3. Ocultar bolsa y mostrar UI
        eventData.pointerDrag.SetActive(false);
        imagenBolsaAbierta.SetActive(true);
        botonReinicio.gameObject.SetActive(true);
    }

    private void GenerarElementosAleatorios()
    {
        for (int i = 0; i < Mathf.Min(cantidadSlots, slotsContainer.childCount); i++)
        {
            if (itemsPosibles.Count == 0) break;

            // Crear nuevo objeto para el ítem
            GameObject itemIcon = new GameObject("ItemIcon");
            itemIcon.transform.SetParent(slotsContainer.GetChild(i));
            itemIcon.transform.localPosition = Vector3.zero;

            // Añadir componentes necesarios
            Image icono = itemIcon.AddComponent<Image>();
            itemIcon.AddComponent<DragItem>();
            itemIcon.AddComponent<CanvasGroup>();

            // Asignar ítem aleatorio
            ItemData item = itemsPosibles[Random.Range(0, itemsPosibles.Count)];
            icono.sprite = item.icon;
            itemIcon.tag = item.itemTag;
        }
    }

    private void LimpiarSlotsCompletamente()
    {
        foreach (Transform slot in slotsContainer)
        {
            // Destruir todos los hijos del slot
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void ReiniciarTotalmente()
    {
        LimpiarSlotsCompletamente();
        imagenBolsaAbierta.SetActive(false);
        botonReinicio.gameObject.SetActive(false);
    }
}