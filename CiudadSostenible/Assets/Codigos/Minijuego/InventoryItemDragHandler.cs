using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración")]
    public GameObject dragVisualPrefab;
    public LayerMask groundMask;
    public float maxDropDistance = 10f;

    [Header("Referencias")]
    public Camera aerialCamera;
    private Camera currentActiveCamera;

    private CanvasGroup canvasGroup;
    private InventorySlot parentSlot;
    private GameObject dragVisual;
    private Vector3 originalPosition;
    private Transform canvasTransform;

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

        if (!EventSystem.current.IsPointerOverGameObject())
            DropItemAtMousePosition();
    }

    private void DropItemAtMousePosition()
    {
        if (parentSlot.IsEmpty()) return;

        ItemData itemData = parentSlot.GetItemData();
        if (itemData == null || itemData.worldPrefab == null) return;

        // Declara 'hit' aquí (antes de usarlo en el if)
        RaycastHit hit;
        Ray ray = currentActiveCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, maxDropDistance, groundMask))
        {
            BowlCapacity bowl = hit.collider.GetComponent<BowlCapacity>();
            if (bowl != null && bowl.TryAddSphere())
            {
                Vector3 spawnPosition = hit.point + Vector3.up * 0.1f;
                GameObject spawnedObject = Instantiate(itemData.worldPrefab, spawnPosition, Quaternion.identity);
                spawnedObject.AddComponent<SwitchPrefabOnClick>().Initialize(itemData);
                parentSlot.RemoveQuantity(1);
            }
        }
        else
        {
            // Opcional: Lógica para cuando el rayo no golpea nada
            Vector3 spawnPosition = ray.origin + ray.direction * maxDropDistance;
            Instantiate(itemData.worldPrefab, spawnPosition, Quaternion.identity);
            parentSlot.RemoveQuantity(1);
        }
    }

    public void SetActiveCamera(Camera activeCamera)
    {
        currentActiveCamera = activeCamera != null ? activeCamera : aerialCamera;
    }
}