using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CanecaReciclaje : MonoBehaviour, IDropHandler
{
    [Header("Configuración")]
    public string[] tagsAceptados; // Ej: {"Papel", "Plastico"} para reciclables
    public int maxItems = 10;
    private int itemsActuales = 0;

    [Header("Referencias UI")]
    public TextMeshProUGUI textoContador;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject itemArrrastrado = eventData.pointerDrag;
        bool esValido = false;

        // Verifica si el tag del ítem está en los tagsAceptados
        foreach (string tag in tagsAceptados)
        {
            if (itemArrrastrado.CompareTag(tag))
            {
                esValido = true;
                break;
            }
        }

        if (esValido)
        {
            itemsActuales++;
            textoContador.text = $"{itemsActuales}/{maxItems}";
            Destroy(itemArrrastrado); // Elimina el ítem

            Debug.Log($"¡Ítem aceptado! Total: {itemsActuales}");
        }
        else
        {
            Debug.Log("¡Ítem incorrecto!");
            itemArrrastrado.GetComponent<DragItem>().ResetPosition(); // Regresa al slot
        }
    }
}