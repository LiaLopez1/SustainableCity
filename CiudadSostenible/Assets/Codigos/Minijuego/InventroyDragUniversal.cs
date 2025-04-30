using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemDragHandlerUniversal : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Visual Drag")]
    public GameObject dragVisualPrefab;

    [Header("Mundo 3D")]
    public LayerMask groundMask;
    public float maxDropDistance = 10f;
    public Camera aerialCamera;

    private CanvasGroup canvasGroup;
    private InventorySlot parentSlot;
    private GameObject dragVisual;
    private Vector3 originalPosition;
    private Transform canvasTransform;
    private Camera currentActiveCamera;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        parentSlot = GetComponentInParent<InventorySlot>();
        canvasTransform = GetComponentInParent<Canvas>().transform;
        currentActiveCamera = aerialCamera;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentSlot == null || parentSlot.IsEmpty()) return;

        originalPosition = transform.position;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        if (dragVisualPrefab != null)
        {
            dragVisual = Instantiate(dragVisualPrefab, canvasTransform);
            dragVisual.GetComponent<Image>().sprite = parentSlot.GetItemData().icon;
            dragVisual.transform.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentSlot.IsEmpty()) return;

        if (dragVisual != null)
            dragVisual.transform.position = Input.mousePosition;
        else
            transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (dragVisual != null)
            Destroy(dragVisual);
        else
            transform.position = originalPosition;

        // Detectamos si cayó en una zona UI válida
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Si el UI tiene algún DropHandler, dejará que él maneje el drop
            // (ej: ZonaBolsaDrop puede implementar IDropHandler)
            return;
        }

        // Si no cayó sobre UI, entonces lanzamos al mundo
        DropItemInWorld();
    }

    private void DropItemInWorld()
    {
        if (parentSlot.IsEmpty()) return;

        ItemData itemData = parentSlot.GetItemData();
        if (itemData == null || itemData.worldPrefab == null) return;

        Ray ray = currentActiveCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDropDistance, groundMask))
        {
            // Ejemplo: si hay un "bowl" como recipiente especial
            BowlCapacity bowl = hit.collider.GetComponent<BowlCapacity>();
            if (bowl != null && bowl.TryAddSphere())
            {
                GameObject spawned = Instantiate(itemData.worldPrefab, hit.point + Vector3.up * 0.1f, Quaternion.identity);
                spawned.AddComponent<SwitchPrefabOnClick>().Initialize(itemData);
                parentSlot.RemoveQuantity(1);
                return;
            }

            // Normal: instancia el prefab
            Instantiate(itemData.worldPrefab, hit.point + Vector3.up * 0.1f, Quaternion.identity);
            parentSlot.RemoveQuantity(1);
        }
    }

    public void SetActiveCamera(Camera camera)
    {
        currentActiveCamera = camera != null ? camera : aerialCamera;
    }
}
