using UnityEngine;

public class TiendaInteraction : MonoBehaviour
{
    [Header("Configuración de la tienda")]
    public int idTienda = 1; // Identificador: 1, 2 o 3

    [Header("Referencias UI")]
    public GameObject canvasTienda;
    public GameObject textoInteraccion;
    public GameObject panelPrincipal;
    public TiendaUIController tiendaUIController;

    private bool jugadorDentro = false;

    void Start()
    {
        if (panelPrincipal != null)
            panelPrincipal.SetActive(false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);
    }

    void Update()
    {
        if (jugadorDentro && Input.GetKeyDown(KeyCode.E))
        {
            tiendaUIController?.EstablecerTiendaActual(idTienda);
            panelPrincipal.SetActive(true);
            textoInteraccion.SetActive(false);
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
