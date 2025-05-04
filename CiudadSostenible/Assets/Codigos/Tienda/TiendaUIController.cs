using UnityEngine;

public class TiendaUIController : MonoBehaviour
{
    [Header("Paneles de compra")]
    public GameObject panelTienda1;
    public GameObject panelTienda2;
    public GameObject panelTienda3;

    [Header("Panel de venta")]
    public GameObject panelVender;

    [Header("Otros")]
    public GameObject panelPrincipal;

    private int tiendaActual = 0; // o 0 como default

    private void Start()
    {
        OcultarTodosLosPaneles();
    }

    public void EstablecerTiendaActual(int id)
    {
        tiendaActual = id;
    }


    public void MostrarPanelTienda(int id)
    {
        OcultarTodosLosPaneles();

        switch (tiendaActual)
        {
            case 1:
                panelTienda1.SetActive(true);
                break;
            case 2:
                panelTienda2.SetActive(true);
                break;
            case 3:
                panelTienda3.SetActive(true);
                break;
        }
    }

    public void MostrarPanelVender()
    {
        OcultarTodosLosPaneles();
        panelVender.SetActive(true);
    }

    public void OcultarTodosLosPaneles()
    {
        panelTienda1.SetActive(false);
        panelTienda2.SetActive(false);
        panelTienda3.SetActive(false);
        panelVender.SetActive(false);
    }

    public void CerrarTienda()
    {
        panelPrincipal.SetActive(false);
        OcultarTodosLosPaneles();
    }
}
