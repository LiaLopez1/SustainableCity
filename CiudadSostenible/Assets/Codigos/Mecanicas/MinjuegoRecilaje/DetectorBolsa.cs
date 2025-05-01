using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DetectorBolsa : MonoBehaviour, IDropHandler
{
    [Header("Referencias UI")]
    public GameObject imagenBolsaAbierta; // Arrastra el panel de la bolsa abierta
    public List<ItemData> itemsPosibles;  // Lista de ítems que pueden aparecer (asígnalos en el Inspector)
    public Transform slotsContainer;      // Objeto padre con los slots (debe tener GridLayoutGroup)

    [Header("Configuración")]
    public int cantidadSlots = 5;        // Número de slots a llenar

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.CompareTag("BolsaBasura"))
        {
            imagenBolsaAbierta.SetActive(true);
            GenerarElementosAleatorios(); // ✨ Nombre correcto del método
            eventData.pointerDrag.SetActive(false); // Opcional: Oculta la bolsa arrastrada
        }
    }

    private void GenerarElementosAleatorios()
    {
        // 1. Limpiar slots anteriores (desactiva los ItemIcon)
        foreach (Transform slot in slotsContainer)
        {
            Transform itemIcon = slot.Find("ItemIcon"); // Busca el hijo "ItemIcon"
            if (itemIcon != null)
            {
                Image icono = itemIcon.GetComponent<Image>();
                icono.sprite = null;
                icono.enabled = false;
                itemIcon.gameObject.tag = "Untagged";
            }
        }

        // 2. Llenar slots con ítems aleatorios
        for (int i = 0; i < Mathf.Min(cantidadSlots, slotsContainer.childCount); i++)
        {
            if (itemsPosibles.Count == 0) break;

            Transform slot = slotsContainer.GetChild(i);
            Transform itemIcon = slot.Find("ItemIcon"); // Asegúrate de que existe!
            if (itemIcon == null)
            {
                Debug.LogError($"No se encontró 'ItemIcon' en el slot {i}");
                continue;
            }

            // Elige un ítem aleatorio
            ItemData itemRandom = itemsPosibles[Random.Range(0, itemsPosibles.Count)];
            Image icono = itemIcon.GetComponent<Image>();

            // Asigna el ícono y el tag
            icono.sprite = itemRandom.icon;
            icono.enabled = true;
            itemIcon.gameObject.tag = itemRandom.itemTag; // Tag al ItemIcon, no al Slot

            Debug.Log($"Slot {i + 1}: {itemRandom.itemName} (Tag: {itemRandom.itemTag})");
        }
    }
}