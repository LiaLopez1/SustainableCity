using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración")]
    public GameObject dragVisualPrefab;
    public LayerMask groundMask;
    public float maxDropDistance = 10f;

    [Header("Efecto de Rebote")]
    public float bounceDuration = 0.5f;
    public float bounceIntensity = 100f;

    [Header("Referencias")]
    public Camera aerialCamera;
    private Camera currentActiveCamera;
    private CanvasGroup canvasGroup;
    private InventorySlot parentSlot;
    private GameObject dragVisual;
    private Vector3 originalPosition;
    private Transform canvasTransform;
    private RectTransform rectTransform;
    private bool isSpecialCanvasActive = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        parentSlot = GetComponentInParent<InventorySlot>();
        canvasTransform = GetComponentInParent<Canvas>().transform;
        rectTransform = GetComponent<RectTransform>();
        currentActiveCamera = aerialCamera;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentSlot == null || parentSlot.IsEmpty()) return;

        originalPosition = rectTransform.position;
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
            rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (dragVisual != null)
            Destroy(dragVisual);
        else
            StartCoroutine(ReturnToSlot());

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (isSpecialCanvasActive)
            {
                DropItemAtMousePosition();
            }
            else
            {
                StartCoroutine(BounceBackToSlot());
            }
        }
    }

    private IEnumerator ReturnToSlot()
    {
        yield return new WaitForEndOfFrame();
        rectTransform.position = originalPosition;
    }

    private IEnumerator BounceBackToSlot()
    {
        Vector3 startPos = rectTransform.position;
        float elapsedTime = 0f;

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / bounceDuration;
            rectTransform.position = Vector3.Lerp(
                startPos,
                originalPosition,
                progress
            ) + Vector3.up * Mathf.Sin(progress * Mathf.PI) * bounceIntensity;
            yield return null;
        }

        rectTransform.position = originalPosition;
    }

    private void DropItemAtMousePosition()
    {
        if (parentSlot.IsEmpty()) return;

        ItemData itemData = parentSlot.GetItemData();
        if (itemData == null || itemData.worldPrefab == null) return;

        RaycastHit hit;
        Ray ray = currentActiveCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, maxDropDistance, groundMask))
        {
            BowlCapacity bowl = hit.collider.GetComponent<BowlCapacity>();
            if (bowl != null && bowl.TryAddSphere())
            {
                Vector3 spawnPosition = hit.point + Vector3.up * 0.1f;
                GameObject spawnedObject = Instantiate(itemData.worldPrefab, spawnPosition, Quaternion.identity);

                spawnedObject.tag = "Esfera"; // Asegúrate de mantener el tag
                SwitchPrefabOnClick switcher = spawnedObject.AddComponent<SwitchPrefabOnClick>();
                switcher.Initialize(itemData, bowl);
                bowl.RegisterSphere(spawnedObject);
                parentSlot.RemoveQuantity(1);
            }
            else
            {
                Debug.Log("❌ Bowl lleno. No se agregó la esfera.");
            }
        }

    }

    public void SetActiveCamera(Camera activeCamera)
    {
        currentActiveCamera = activeCamera != null ? activeCamera : aerialCamera;
    }

    public void SetSpecialCanvasActive(bool isActive)
    {
        isSpecialCanvasActive = isActive;
    }
}
