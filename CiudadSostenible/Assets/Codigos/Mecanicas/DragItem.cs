using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Referencias
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image itemImage;
    private Transform parentOriginal; // Guarda el padre original para resetear posici�n

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        itemImage = GetComponent<Image>();
        parentOriginal = transform.parent; // El Slot padre
    }

    // 1. Cuando comienza el arrastre
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f; // Hace el �tem semi-transparente
        canvasGroup.blocksRaycasts = false; // Permite detectar el drop en las canecas

        // Opcional: Cambia el padre temporalmente al Canvas para que est� encima de todo
        transform.SetParent(GetComponentInParent<Canvas>().transform);
    }

    // 2. Durante el arrastre
    public void OnDrag(PointerEventData eventData)
    {
        // Mueve el �tem con el mouse (coordenadas convertidas al espacio del Canvas)
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }

    // 3. Cuando termina el arrastre
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Si no se solt� sobre una caneca, regresa al slot original
        if (eventData.pointerEnter == null || !eventData.pointerEnter.CompareTag("Caneca"))
        {
            ResetPosition();
        }
    }

    // M�todo para resetear la posici�n al slot original
    public void ResetPosition()
    {
        transform.SetParent(parentOriginal);
        rectTransform.localPosition = Vector3.zero; // Centrado en el slot
    }
}