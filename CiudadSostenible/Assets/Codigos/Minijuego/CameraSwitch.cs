using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelUI;
    public Camera mainCamera;
    public Camera specialCamera;
    public PlayerMove playerMovement; // Referencia al controlador de movimiento

    [Header("Configuración")]
    public KeyCode switchKey = KeyCode.E;
    public KeyCode exitKey = KeyCode.Escape;

    private bool canSwitch = false;

    private void Start()
    {
        mainCamera.enabled = true;
        specialCamera.enabled = false;
        panelUI.SetActive(false);

        // Asegurarse de que el jugador puede moverse al inicio
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
        if (canSwitch && Input.GetKeyDown(switchKey) && !specialCamera.enabled)
        {
            SwitchToSpecialCamera();
        }

        if (Input.GetKeyDown(exitKey))
        {
            ReturnToMainCamera();
        }
    }

    private void SwitchToSpecialCamera()
    {
        mainCamera.enabled = false;
        specialCamera.enabled = true;
        panelUI.SetActive(false);

        // Deshabilitar movimiento del jugador
        if (playerMovement != null)
            playerMovement.EnableMovement(false);
    }

    private void ReturnToMainCamera()
    {
        mainCamera.enabled = true;
        specialCamera.enabled = false;
        panelUI.SetActive(false);
        canSwitch = false;

        // Habilitar movimiento del jugador
        if (playerMovement != null)
            playerMovement.EnableMovement(true);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ReturnToMainCamera();
        }
    }
}