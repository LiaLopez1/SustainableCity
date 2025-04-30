using UnityEngine;
using UnityEngine.UI; // Necesario si usas Texto UI

public class AbrirMinijuegoC : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelMinijuego;      // Panel del minijuego (ej: "PanelMinijuego")
    public GameObject panelInventario;     // Panel del inventario (ej: "PanelInventario")
    public GameObject textoInteraccion;    // Texto "Presiona E" (como GameObject)

    [Header("Configuraci�n")]
    public KeyCode teclaInteraccion = KeyCode.E;

    private bool jugadorEnRango = false;

    void Start()
    {
        panelMinijuego.SetActive(false);
        if (textoInteraccion != null) textoInteraccion.SetActive(false);
    }

    void Update()
    {
        if (jugadorEnRango && Input.GetKeyDown(teclaInteraccion))
        {
            // Activa el panel del minijuego y asegura que el inventario est� visible
            panelMinijuego.SetActive(true);
            panelInventario.SetActive(true);

            // Opcional: Forzar el orden visual si hay superposici�n
            Canvas canvasPadre = panelInventario.GetComponentInParent<Canvas>();
            if (canvasPadre != null) canvasPadre.sortingOrder = 2;

            if (textoInteraccion != null) textoInteraccion.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = true;
            if (textoInteraccion != null) textoInteraccion.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = false;
            panelMinijuego.SetActive(false);
            if (textoInteraccion != null) textoInteraccion.SetActive(false);
        }
    }
}