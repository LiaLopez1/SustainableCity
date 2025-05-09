using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelUI;
    public Canvas specialViewCanvas;
    public Camera mainCamera;
    public PlayerMove playerMovement;

    [Header("Configuración")]
    public KeyCode switchKey = KeyCode.E;

    private bool canSwitch = false;
    private Camera specialCamera;
    private ProgresoMundo progresoMundo;

    private void Start()
    {
        progresoMundo = FindObjectOfType<ProgresoMundo>();

        mainCamera.enabled = true;
        specialViewCanvas.gameObject.SetActive(false);

        specialCamera = specialViewCanvas.GetComponentInChildren<Camera>();
        if (specialCamera != null)
            specialCamera.enabled = false;

        if (playerMovement != null)
            playerMovement.EnableMovement(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (progresoMundo != null && progresoMundo.MaquinaEstaDesbloqueada(gameObject))
            {
                panelUI.SetActive(true);
                canSwitch = true;
            }
            else
            {
                panelUI.SetActive(false);
                canSwitch = false;
                Debug.Log($"{gameObject.name} está bloqueada. No se puede mostrar el panel.");
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ReturnToMainView();
        }
    }

    private void Update()
    {
        if (canSwitch && Input.GetKeyDown(switchKey))
        {
            if (specialViewCanvas.gameObject.activeSelf)
                ReturnToMainView();
            else
                SwitchToSpecialView();
        }
    }

    private void SwitchToSpecialView()
    {
        specialViewCanvas.gameObject.SetActive(true);
        if (specialCamera != null)
            specialCamera.enabled = true;

        panelUI.SetActive(false);
        if (playerMovement != null)
            playerMovement.EnableMovement(false);

        NotifyDragHandlers(true);
    }

    private void ReturnToMainView()
    {
        specialViewCanvas.gameObject.SetActive(false);
        if (specialCamera != null)
            specialCamera.enabled = false;

        mainCamera.enabled = true;
        panelUI.SetActive(false);
        canSwitch = false;

        if (playerMovement != null)
            playerMovement.EnableMovement(true);

        NotifyDragHandlers(false);
    }

    private void NotifyDragHandlers(bool isActive)
    {
        foreach (var handler in FindObjectsOfType<InventoryItemDragHandler>())
        {
            handler.SetSpecialCanvasActive(isActive);
        }
    }
}
