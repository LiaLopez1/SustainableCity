using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TiendaInteraction : MonoBehaviour
{
    [Header("Paneles de la tienda")]
    public GameObject panelBotonesIniciales;     // Panel con botones "Abrir tienda" y "Salir"
    public GameObject panelContenidoTienda;      // Panel que muestra el contenido de la tienda

    [Header("Fade")]
    public CanvasGroup canvasGroupFade;

    [Header("Cámaras")]
    public Camera camaraPrincipal;
    public Camera camaraSecundaria;
    public GameObject objetoCamaraSecundaria;

    [Header("Otros elementos UI a ocultar")]
    public GameObject[] objetosUIAOcultar;

    [Header("Movimiento del jugador")]
    public MonoBehaviour scriptMovimientoJugador;

    [Header("Texto de interacción")]
    public GameObject textoInteraccion;

    [Header("Botones")]
    public Button botonAbrirTienda;
    public Button botonCerrarTienda;

    private bool jugadorDentro = false;
    private bool interactuando = false;

    void Start()
    {
        panelBotonesIniciales?.SetActive(false);
        panelContenidoTienda?.SetActive(false);
        textoInteraccion?.SetActive(false);

        if (canvasGroupFade != null)
        {
            canvasGroupFade.alpha = 0f;
            canvasGroupFade.gameObject.SetActive(false);
        }

        camaraSecundaria.enabled = false;
        if (objetoCamaraSecundaria != null)
            objetoCamaraSecundaria.SetActive(false);

        if (botonAbrirTienda != null)
            botonAbrirTienda.onClick.AddListener(AbrirPanelTienda);

        if (botonCerrarTienda != null)
            botonCerrarTienda.onClick.AddListener(CerrarTienda);
    }

    void Update()
    {
        if (jugadorDentro && !interactuando && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(IniciarInteraccion());
        }
    }

    IEnumerator IniciarInteraccion()
    {
        interactuando = true;
        textoInteraccion?.SetActive(false);

        canvasGroupFade.gameObject.SetActive(true);

        // Desactivar movimiento del jugador
        if (scriptMovimientoJugador != null)
            scriptMovimientoJugador.enabled = false;

        // Fade In
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, 0.5f));

        // Esperar pantalla negra
        yield return new WaitForSeconds(0.5f);

        // Activar cámara secundaria
        if (objetoCamaraSecundaria != null)
            objetoCamaraSecundaria.SetActive(true);

        camaraPrincipal.enabled = false;
        camaraSecundaria.enabled = true;

        // Ocultar otros elementos UI
        foreach (GameObject obj in objetosUIAOcultar)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Mostrar panel con botones
        panelBotonesIniciales?.SetActive(true);
        panelContenidoTienda?.SetActive(false);

        // Fade Out
        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, 0.5f));
        canvasGroupFade.gameObject.SetActive(false);
    }

    public void AbrirPanelTienda()
    {
        panelContenidoTienda?.SetActive(true);
    }

    public void CerrarTienda()
    {
        StartCoroutine(SalirDeInteraccion());
    }

    IEnumerator SalirDeInteraccion()
    {
        canvasGroupFade.gameObject.SetActive(true);

        // Fade In
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, 0.5f));
        yield return new WaitForSeconds(0.5f);

        // Apagar cámara secundaria
        if (objetoCamaraSecundaria != null)
            objetoCamaraSecundaria.SetActive(false);

        camaraSecundaria.enabled = false;
        camaraPrincipal.enabled = true;

        // Reactivar movimiento
        if (scriptMovimientoJugador != null)
            scriptMovimientoJugador.enabled = true;

        // Reactivar otros UI
        foreach (GameObject obj in objetosUIAOcultar)
        {
            if (obj != null) obj.SetActive(true);
        }

        // Ocultar paneles
        panelBotonesIniciales?.SetActive(false);
        panelContenidoTienda?.SetActive(false);

        // Fade Out
        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, 0.5f));
        canvasGroupFade.gameObject.SetActive(false);

        interactuando = false;
    }

    IEnumerator FadeCanvasGroup(float alphaInicio, float alphaFin, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(alphaInicio, alphaFin, t / duracion);
            canvasGroupFade.alpha = a;
            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;
            if (!interactuando)
                textoInteraccion?.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            textoInteraccion?.SetActive(false);

            if (interactuando)
                CerrarTienda();
        }
    }
}
