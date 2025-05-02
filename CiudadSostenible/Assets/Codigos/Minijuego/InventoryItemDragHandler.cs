using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class InventoryItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración visual")]
    public GameObject dragVisualPrefab;
    public float maxDropDistance = 10f;
    public LayerMask groundMask;

    [Header("Efecto de Rebote")]
    public float bounceDuration = 0.5f;
    public float bounceIntensity = 100f;

    [Header("Cámaras válidas para raycast")]
    public List<Camera> allowedCameras = new List<Camera>();
    private Camera currentActiveCamera;

    [Header("Referencias")]
    private CanvasGroup canvasGroup;
    private InventorySlot parentSlot;
    private GameObject dragVisual;
    private Vector3 originalPosition;
    private Transform canvasTransform;
    private RectTransform rectTransform;
    private bool isSpecialCanvasActive = false;
    private SphereDropHandler sphereDropHandler;

    [Header("Punto fijo para botellas")]
    public Transform botellaSpawnPoint;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        parentSlot = GetComponentInParent<InventorySlot>();
        canvasTransform = GetComponentInParent<Canvas>().transform;
        rectTransform = GetComponent<RectTransform>();
        sphereDropHandler = GetComponent<SphereDropHandler>();
        currentActiveCamera = GetActiveCameraFromList();
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
            ItemData itemData = parentSlot.GetItemData();

            if (isSpecialCanvasActive && itemData != null)
            {
                UpdateCurrentActiveCamera();

                if (itemData.itemTag == "Botella" && botellaSpawnPoint != null)
                {
                    // Instanciar la botella en el punto fijo
                    GameObject spawned = Instantiate(itemData.worldPrefab, botellaSpawnPoint.position, Quaternion.identity);
                    spawned.tag = "Botella";
                    Debug.Log($"✅ Botella instanciada en {botellaSpawnPoint.position}");
                    parentSlot.RemoveQuantity(1);
                }
                else if (sphereDropHandler != null)
                {
                    // Usar lógica de raycast para esferas u otros
                    sphereDropHandler.DropItemAtMousePosition(parentSlot, currentActiveCamera, groundMask, maxDropDistance);
                }
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
            rectTransform.position = Vector3.Lerp(startPos, originalPosition, progress)
                + Vector3.up * Mathf.Sin(progress * Mathf.PI) * bounceIntensity;
            yield return null;
        }

        rectTransform.position = originalPosition;
    }

    private Camera GetActiveCameraFromList()
    {
        foreach (Camera cam in allowedCameras)
        {
            if (cam != null && cam.enabled && cam.gameObject.activeInHierarchy)
                return cam;
        }

        Debug.LogWarning("⚠️ No hay cámaras activas en la lista. Usando la primera como fallback.");
        return allowedCameras.Count > 0 ? allowedCameras[0] : null;
    }

    public void UpdateCurrentActiveCamera()
    {
        currentActiveCamera = GetActiveCameraFromList();
    }

    public void SetSpecialCanvasActive(bool isActive)
    {
        isSpecialCanvasActive = isActive;
    }
}
