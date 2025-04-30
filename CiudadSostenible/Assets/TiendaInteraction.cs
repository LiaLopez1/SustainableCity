using UnityEngine;

public class TiendaInteraction : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject canvasTienda;         // Aquí sigue y se queda activo todo el tiempo
    public GameObject textoInteraccion;     // Texto que aparece al acercarse
    public GameObject panelPrincipal;       // Este es el que contiene los botones y paneles
    public TiendaUIController tiendaUIController;

    private bool jugadorDentro = false;

    void Start()
    {
        if (panelPrincipal != null)
            panelPrincipal.SetActive(false); // Aunque esté activo en el editor, lo apagamos aquí

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);
    }

    void Update()
    {
        if (jugadorDentro && Input.GetKeyDown(KeyCode.E))
        {
            panelPrincipal.SetActive(true);
            textoInteraccion.SetActive(false);
            tiendaUIController?.OcultarTodosLosPaneles();
        }

        if (panelPrincipal.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            tiendaUIController?.CerrarTienda();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;
            textoInteraccion?.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            textoInteraccion?.SetActive(false);
            tiendaUIController?.CerrarTienda();
        }
    }
}
