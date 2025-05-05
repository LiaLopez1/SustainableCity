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

    [Header("Configuración papel")]
    public Transform paperSpawnPoint;
    public Transform paperDestinoSpawnPoint;
    public TextMeshProUGUI avisoPaperTMP;
    private GameObject paperEnSpawn = null;

    [Header("Manager no aprovechables")]
    public NoAprovechablesManager noAprovechablesManager;

    private GameObject botellaActivaEnSpawn = null;

    public InventorySlot GetParentSlot() => parentSlot;

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

        if (objetoSoltado != null && objetoSoltado.CompareTag("Caneca"))
        {
            CanecaReciclaje caneca = objetoSoltado.GetComponent<CanecaReciclaje>();
            if (caneca != null) return;
        }

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            ItemData itemData = parentSlot.GetItemData();

            if (isSpecialCanvasActive && itemData != null)
            {
                UpdateCurrentActiveCamera();

                if (itemData.itemTag == "Botella" && botellaSpawnPoint != null && IsSecondCameraActive())
                {
                    if (botellaActivaEnSpawn != null)
                    {
                        if (avisoPaperTMP != null)
                        {
                            avisoPaperTMP.gameObject.SetActive(true);
                            StartCoroutine(HideWarningTMP(avisoPaperTMP, 2f));
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
                else if (itemData.itemTag == "Paper" && paperSpawnPoint != null && IsThirdCameraActive())
                {
                    if (paperEnSpawn != null)
                    {
                        if (avisoPaperTMP != null)
                        {
                            avisoPaperTMP.gameObject.SetActive(true);
                            StartCoroutine(HideWarningTMP(avisoPaperTMP, 2f));
                        }
                        StartCoroutine(BounceBackToSlot());
                        return;
                    }

                    GameObject papel = Instantiate(itemData.worldPrefab, paperSpawnPoint.position, Quaternion.identity);
                    papel.tag = "Paper";
                    parentSlot.RemoveQuantity(1);
                    paperEnSpawn = papel;

                    PaperClickSplitter paperLogic = papel.GetComponent<PaperClickSplitter>();
                    if (paperLogic != null)
                    {
                        paperLogic.paperFinalSpawnPoint = paperDestinoSpawnPoint;
                        paperLogic.onPaperCompletado = () => paperEnSpawn = null;
                    }
                }
                else if (itemData.itemTag == "NoAprovechables" && IsFourthCameraActive())
                {
                    if (noAprovechablesManager == null)
                    {
                        Debug.LogWarning("⚠️ No se asignó el NoAprovechablesManager.");
                        StartCoroutine(BounceBackToSlot());
                        return;
                    }

                    if (!noAprovechablesManager.PuedeRecibirObjeto())
                    {
                        StartCoroutine(BounceBackToSlot());
                        return;
                    }

                    Vector3 spawnPos = noAprovechablesManager.CalcularPosicionSpawn();

                    GameObject basura = Instantiate(
                        itemData.worldPrefab,
                        spawnPos,
                        Quaternion.Euler(-90f, 0f, 0f)
                    );

                    basura.tag = "NoAprovechables";
                    parentSlot.RemoveQuantity(1);
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
        StartCoroutine(BounceBackToSlot());
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

    public bool IsFirstCameraActive()
    {
        return allowedCameras.Count >= 1 && allowedCameras[0].enabled && allowedCameras[0].gameObject.activeInHierarchy;
    }

    public bool IsSecondCameraActive()
    {
        return allowedCameras.Count >= 2 && allowedCameras[1].enabled && allowedCameras[1].gameObject.activeInHierarchy;
    }

    public bool IsThirdCameraActive()
    {
        return allowedCameras.Count >= 3 && allowedCameras[2].enabled && allowedCameras[2].gameObject.activeInHierarchy;
    }

    public bool IsFourthCameraActive()
    {
        return allowedCameras.Count >= 4 && allowedCameras[3].enabled && allowedCameras[3].gameObject.activeInHierarchy;
    }

    private IEnumerator HideWarningTMP(TextMeshProUGUI tmp, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (tmp != null)
            tmp.gameObject.SetActive(false);
    }
}
