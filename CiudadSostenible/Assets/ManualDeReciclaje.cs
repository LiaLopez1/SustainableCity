using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ManualDeReciclaje : MonoBehaviour
{
    [Header("Configuración del manual")]
    public ItemData manualItem;
    public InventorySystem inventario;
    public GameObject mensajeUI;
    public GameObject panelManual;
    public List<GameObject> paginas = new List<GameObject>();

    [Header("Botones de navegación")]
    public Button botonSiguiente;
    public Button botonAnterior;

    private int paginaActual = 0;
    private bool manualAbierto = false;

    void Start()
    {
        mensajeUI?.SetActive(false);
        panelManual?.SetActive(false);

       

        DesactivarTodasLasPaginas();
    }

    void Update()
    {
        if (inventario != null && inventario.TieneItem(manualItem, 1))
        {
            mensajeUI?.SetActive(true);

            if (Input.GetKeyDown(KeyCode.M))
            {
                manualAbierto = !manualAbierto;
                panelManual.SetActive(manualAbierto);

                if (manualAbierto)
                {
                    paginaActual = 0;
                    ActivarPagina(paginaActual);
                }
                else
                {
                    DesactivarTodasLasPaginas();
                }
            }
        }
        else
        {
            mensajeUI?.SetActive(false);
            panelManual?.SetActive(false);
            DesactivarTodasLasPaginas();
        }
    }

    void ActivarPagina(int indice)
    {
        DesactivarTodasLasPaginas();

        if (indice >= 0 && indice < paginas.Count)
            paginas[indice].SetActive(true);

        // Controla la navegación
        botonAnterior.interactable = indice > 0;
        botonSiguiente.interactable = indice < paginas.Count - 1;
    }

    void DesactivarTodasLasPaginas()
    {
        foreach (var pagina in paginas)
        {
            if (pagina != null)
                pagina.SetActive(false);
        }
    }

    public void PasarPagina()
    {
        if (paginaActual < paginas.Count - 1)
        {
            paginaActual++;
            ActivarPagina(paginaActual);
        }
    }

    public void PaginaAnterior()
    {
        if (paginaActual > 0)
        {
            paginaActual--;
            ActivarPagina(paginaActual);
        }
    }
}

