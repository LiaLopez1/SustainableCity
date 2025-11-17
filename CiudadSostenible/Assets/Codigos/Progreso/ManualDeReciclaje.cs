using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ManualDeReciclaje : MonoBehaviour
{
    [Header("Configuración del manual")]
    public ItemData manualItem;
    public InventorySystem inventario;
    public GameObject mensajeUI;          // Texto como "Press M..." (puedes eliminarlo si ya no lo usas)
    public GameObject panelManual;
    public List<GameObject> paginas = new List<GameObject>();

    [Header("Botones de navegación")]
    public Button botonSiguiente;
    public Button botonAnterior;

    [Header("Botón para abrir/cerrar manual")]
    public Button botonAbrirCerrarManual;

    [Header("Retroalimentación visual inicial")]
    public GameObject textoIntroduccionManual;  // Texto tipo "puedes revisar el libro..."
    public GameObject flechaUI;                 // Imagen tipo flecha

    private int paginaActual = 0;
    private bool manualActivo = false;
    private bool manualFueEntregado = false;

    void Start()
    {
        mensajeUI?.SetActive(false);
        panelManual?.SetActive(false);
        botonAbrirCerrarManual?.gameObject.SetActive(false);

        DesactivarTodasLasPaginas();

        if (botonAbrirCerrarManual != null)
            botonAbrirCerrarManual.onClick.AddListener(ToggleManual);
    }

    void Update()
    {
        // Solo detecta una vez si el manual fue entregado
        if (!manualFueEntregado && inventario != null && inventario.TieneItem(manualItem, 1))
        {
            manualFueEntregado = true;
            inventario.RemoveItem(manualItem, 1); // Elimina el manual

            botonAbrirCerrarManual?.gameObject.SetActive(true);

            // Activar retroalimentación visual
            if (textoIntroduccionManual != null) textoIntroduccionManual.SetActive(true);
            if (flechaUI != null) flechaUI.SetActive(true);
            Invoke(nameof(DesactivarFeedbackManual), 4f);
        }
    }


    void ToggleManual()
    {
        manualActivo = !manualActivo;
        panelManual.SetActive(manualActivo);

        if (manualActivo)
        {
            paginaActual = 0;
            ActivarPagina(paginaActual);
        }
        else
        {
            DesactivarTodasLasPaginas();
        }
    }

    void ActivarPagina(int indice)
    {
        DesactivarTodasLasPaginas();

        if (indice >= 0 && indice < paginas.Count)
            paginas[indice].SetActive(true);

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

    void DesactivarFeedbackManual()
    {
        if (textoIntroduccionManual != null) textoIntroduccionManual.SetActive(false);
        if (flechaUI != null) flechaUI.SetActive(false);
    }

}
