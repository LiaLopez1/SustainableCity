using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform parentOriginal;
    private Image itemImage;
    private InventorySlot slotOriginal;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        itemImage = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentOriginal = transform.parent;
        slotOriginal = parentOriginal.GetComponent<InventorySlot>(); // Guardar referencia al slot

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(GetComponentInParent<Canvas>().transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("Caneca"))
        {
            if (eventData.pointerEnter.name.Contains(gameObject.tag))
            {
                // 🧹 Limpiar slot antes de destruir
                if (slotOriginal != null)
                {
                    slotOriginal.ClearSlot();
                }

                Destroy(gameObject);
                Debug.Log($"Ítem {gameObject.tag} eliminado en caneca correcta");
            }
            else
            {
                ResetPosition();
                Debug.LogWarning("¡Caneca incorrecta!");
            }
        }
        else
        {
            ResetPosition();
        }
    }

    private void ResetPosition()
    {
        transform.SetParent(parentOriginal);
        rectTransform.localPosition = Vector3.zero;
    }
}
