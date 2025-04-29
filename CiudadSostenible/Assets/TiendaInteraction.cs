using UnityEngine;
using UnityEngine.UI;

public class TiendaInteraction : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject canvasTienda;         // Panel con botones de Vender y Comprar
    public GameObject textoInteraccion;     // Texto: "Presiona E para interactuar"

    private bool jugadorDentro = false;     // Saber si el jugador está dentro del trigger

    void Start()
    {
        if (canvasTienda != null)
            canvasTienda.SetActive(false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);
    }

    void Update()
    {
        if (jugadorDentro && Input.GetKeyDown(KeyCode.E))
        {
            canvasTienda.SetActive(true);
            textoInteraccion.SetActive(false); // Oculta el texto al abrir el menú
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;

            if (textoInteraccion != null)
                textoInteraccion.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;

            if (textoInteraccion != null)
                textoInteraccion.SetActive(false);

            if (canvasTienda != null)
                canvasTienda.SetActive(false); // Cierra el canvas si se va
        }
    }
}
