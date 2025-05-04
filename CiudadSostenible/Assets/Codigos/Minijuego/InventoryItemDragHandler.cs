
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
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

    [Header("Puntos de Spawn")]
    public Transform botellaSpawnPoint;
    public Transform tapaSpawnPoint;
    public Transform botellaDestinoSpawnPoint;

    [Header("Mensaje UI")]
    public TextMeshProUGUI avisoTMP;

    private GameObject botellaActivaEnSpawn = null;


    public InventorySlot GetParentSlot()
    {
        return parentSlot;
    }
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

        GameObject objetoSoltado = eventData.pointerCurrentRaycast.gameObject;

        // Caso 1: Se soltó sobre una caneca válida
        if (objetoSoltado != null && objetoSoltado.CompareTag("Caneca"))
        {
            CanecaReciclaje caneca = objetoSoltado.GetComponent<CanecaReciclaje>();
            if (caneca != null)
            {
                // La caneca ya manejará la lógica (restar cantidad, etc.)
                return; // No hagas nada más, CanecaReciclaje se encarga
            }
        }

        // Caso 2: No es una caneca (o es inválida)
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // [Tu lógica existente para spawn en el mundo...]
        }
        else
        {
            // Vuelve al slot original (rebote o movimiento suave)
            StartCoroutine(BounceBackToSlot());
        }



        // Caso especial para bolsas de basura
        if (parentSlot.GetItemData().itemTag == "BolsaBasura" &&
            eventData.pointerCurrentRaycast.gameObject != null &&
            eventData.pointerCurrentRaycast.gameObject.GetComponent<DetectorBolsa>() != null)
        {
            // El DetectorBolsa ahora manejará la resta de cantidad
            return; // Salimos sin hacer el rebote
        }



        if (!EventSystem.current.IsPointerOverGameObject())
        {
            ItemData itemData = parentSlot.GetItemData();

            if (isSpecialCanvasActive && itemData != null)
            {
                UpdateCurrentActiveCamera();

                if (itemData.itemTag == "Botella" && botellaSpawnPoint != null && IsSecondCameraActive())
                {
                    if (IsBottleAlreadyInSpawn())
                    {
                        if (avisoTMP != null)
                        {
                            avisoTMP.gameObject.SetActive(true);
                            StartCoroutine(HideWarningAfterSeconds(2f));
                        }
                        StartCoroutine(BounceBackToSlot());
                        return;
                    }

                    GameObject spawned = Instantiate(itemData.worldPrefab, botellaSpawnPoint.position, Quaternion.identity);
                    spawned.tag = "Botella";
                    parentSlot.RemoveQuantity(1);
                    botellaActivaEnSpawn = spawned;

                    BottleClickHandler handler = spawned.GetComponent<BottleClickHandler>();
                    if (handler != null)
                    {
                        handler.tapaSpawnPoint = tapaSpawnPoint;
                        handler.bottleFinalSpawnPoint = botellaDestinoSpawnPoint;
                        handler.onBotellaCompletada = () => botellaActivaEnSpawn = null;
                    }
                }
                else if (itemData.itemTag == "Esfera" && IsFirstCameraActive() && sphereDropHandler != null)
                {
                    sphereDropHandler.DropItemAtMousePosition(parentSlot, currentActiveCamera, groundMask, maxDropDistance);
                }
                else
                {
                    StartCoroutine(BounceBackToSlot());
                }
            }
            else
            {
                StartCoroutine(BounceBackToSlot());
            }
        }
    }
    public void ReturnToInventory()
    {
        StartCoroutine(BounceBackToSlot()); // Usa tu coroutine existente
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

    public bool IsSecondCameraActive()
    {
        if (allowedCameras.Count >= 2)
        {
            Camera cam = allowedCameras[1];
            return cam != null && cam.enabled && cam.gameObject.activeInHierarchy;
        }
        return false;
    }

    public bool IsFirstCameraActive()
    {
        if (allowedCameras.Count >= 1)
        {
            Camera cam = allowedCameras[0];
            return cam != null && cam.enabled && cam.gameObject.activeInHierarchy;
        }
        return false;
    }

    private bool IsBottleAlreadyInSpawn()
    {
        return botellaActivaEnSpawn != null;
    }

    private IEnumerator HideWarningAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (avisoTMP != null)
        {
            avisoTMP.gameObject.SetActive(false);
        }
    }
}
