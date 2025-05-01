using UnityEngine;

public class InteraccionCanecas : MonoBehaviour
{
    public GameObject mensajeInteraccion;  // Mensaje "Presiona E".
    public GameObject panelMiniJuego;     // Panel del minijuego.
    public GameObject panelInventario;    // Panel del inventario (ya activo).

    private bool jugadorCerca = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            mensajeInteraccion.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            mensajeInteraccion.SetActive(false);
            panelMiniJuego.SetActive(false);
        }
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            panelMiniJuego.SetActive(!panelMiniJuego.activeSelf);
            // Asegura que el inventario esté activo y visible:
            panelInventario.SetActive(true);
        }
    }
}