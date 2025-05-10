using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelUI;
    public Canvas specialViewCanvas;
    public Camera mainCamera;
    public PlayerMove playerMovement;
    public GameObject botonCerrar;

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

        if (botonCerrar != null)
            botonCerrar.SetActive(false);
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
            panelUI.SetActive(false);
            canSwitch = false;

            // NOTA: No llamamos a ReturnToMainView aquí para NO cerrar la cámara automáticamente
        }
    }

    private void Update()
    {
        // ✅ Solo se puede entrar con E si el canvas no está activo aún
        if (canSwitch && Input.GetKeyDown(switchKey) && !specialViewCanvas.gameObject.activeSelf)
        {
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

        if (botonCerrar != null)
            botonCerrar.SetActive(true);

        NotifyDragHandlers(true);
    }

    // 🔴 Solo se llama desde el botón
    public void ReturnToMainView()
    {
        specialViewCanvas.gameObject.SetActive(false);
        if (specialCamera != null)
            specialCamera.enabled = false;

        mainCamera.enabled = true;

        if (playerMovement != null)
            playerMovement.EnableMovement(true);

        if (botonCerrar != null)
            botonCerrar.SetActive(false);

        canSwitch = false;

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
