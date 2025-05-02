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
    public KeyCode exitKey = KeyCode.Escape;

    private bool canSwitch = false;
    private Camera specialCamera;

    private void Start()
    {
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
            panelUI.SetActive(true);
            canSwitch = true;
        }
    }

    private void Update()
    {
        if (canSwitch && Input.GetKeyDown(switchKey) && !specialViewCanvas.gameObject.activeSelf)
            SwitchToSpecialView();

        if (Input.GetKeyDown(exitKey) && specialViewCanvas.gameObject.activeSelf)
            ReturnToMainView();
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

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            ReturnToMainView();
    }

    private void NotifyDragHandlers(bool isActive)
    {
        // Notificar a los arrastrables
        foreach (var handler in FindObjectsOfType<InventoryItemDragHandler>())
            handler.SetSpecialCanvasActive(isActive);

        // Notificar a las tapas de botella
        foreach (var cap in FindObjectsOfType<BottleCap>())
            cap.SetCamaraEspecialActiva(isActive);
    }
}
