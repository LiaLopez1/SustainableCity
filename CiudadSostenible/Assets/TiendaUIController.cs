using UnityEngine;

public class TiendaUIController : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelComprar;
    public GameObject panelVender;
    public GameObject canvasTienda;
    public GameObject panelPrincipal;

    private void Start()
    {
        OcultarTodosLosPaneles();
    }

    public void MostrarPanelComprar()
    {
        panelComprar.SetActive(true);
        panelVender.SetActive(false);
    }

    public void MostrarPanelVender()
    {
        panelVender.SetActive(true);
        panelComprar.SetActive(false);
    }

    public void OcultarTodosLosPaneles()
    {
        panelComprar.SetActive(false);
        panelVender.SetActive(false);
    }

    public void CerrarTienda()
    {
        panelPrincipal.SetActive(false);
        OcultarTodosLosPaneles();
    }

}
