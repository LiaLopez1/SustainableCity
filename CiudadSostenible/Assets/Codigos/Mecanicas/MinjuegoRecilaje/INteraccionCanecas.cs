using UnityEngine;

public class InteraccionCanecas : MonoBehaviour
{
    public GameObject mensajeInteraccion;  // Imagen "Open E"
    public GameObject panelMiniJuego;      // Panel del minijuego
    public GameObject panelInventario;     // Inventario (se asegura de estar activo)

    private bool jugadorCerca = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            mensajeInteraccion.SetActive(true);  // Mostrar el mensaje
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            mensajeInteraccion.SetActive(false); // Ocultar el mensaje
            panelMiniJuego.SetActive(false);     // Cerrar panel si se aleja
        }
    }

    void Update()
    {
        // Abrir con E si está cerca y el panel no está abierto
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            panelMiniJuego.SetActive(true);
            panelInventario.SetActive(true);
            mensajeInteraccion.SetActive(false); // Ocultar "Open E" cuando entra
        }

        // Cerrar con Escape si el panel está abierto
        if (panelMiniJuego.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            panelMiniJuego.SetActive(false);
            if (jugadorCerca)
                mensajeInteraccion.SetActive(true);  // Mostrar el mensaje de nuevo si sigue cerca
        }
    }
}
