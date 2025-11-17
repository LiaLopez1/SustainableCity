using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TiendaInteraction : MonoBehaviour
{
    [Header("Paneles de la tienda")]
    public GameObject panelBotonesIniciales;
    public GameObject panelContenidoTienda;

    [Header("Fade")]
    public CanvasGroup canvasGroupFade;

    [Header("Cámaras")]
    public Camera camaraPrincipal;
    public Camera camaraSecundaria;
    public GameObject objetoCamaraSecundaria;

    [Header("Otros elementos UI a ocultar")]
    public GameObject[] objetosUIAOcultar;

    [Header("Movimiento del jugador")]
    public PlayerMovement scriptMovimientoJugador;

    [Header("Texto de interacción")]
    public GameObject textoInteraccion;

    [Header("Botones")]
    public Button botonAbrirTienda;
    public Button botonCerrarTienda;

    private bool jugadorDentro = false;
    private bool interactuando = false;
    private ShopContext _ctx;

    void Start()
    {
        _ctx = GetComponent<ShopContext>();

        panelBotonesIniciales?.SetActive(false);
        panelContenidoTienda?.SetActive(false);
        textoInteraccion?.SetActive(false);

        if (canvasGroupFade != null)
        {
            canvasGroupFade.alpha = 0f;
            canvasGroupFade.gameObject.SetActive(false);
        }

        if (camaraSecundaria != null)
            camaraSecundaria.enabled = false;

        if (objetoCamaraSecundaria != null)
            objetoCamaraSecundaria.SetActive(false);

        // 🔹 Eliminado: no se asignan listeners aquí.
        // Ahora los conectas directamente desde el Inspector.
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

        if (scriptMovimientoJugador != null)
            scriptMovimientoJugador.enabled = false;

        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, 0.5f));
        yield return new WaitForSeconds(0.5f);

        if (objetoCamaraSecundaria != null)
            objetoCamaraSecundaria.SetActive(true);

        if (camaraPrincipal != null) camaraPrincipal.enabled = false;
        if (camaraSecundaria != null) camaraSecundaria.enabled = true;

        foreach (GameObject obj in objetosUIAOcultar)
            if (obj != null) obj.SetActive(false);

        panelBotonesIniciales?.SetActive(true);
        panelContenidoTienda?.SetActive(false);

        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, 0.5f));
        canvasGroupFade.gameObject.SetActive(false);
    }

    // === Métodos públicos ===

    public void AbrirPanelTienda()
    {
        _ctx?.OnOpenStore();
        panelContenidoTienda?.SetActive(true);
    }

    public void CerrarTienda()
    {
        StartCoroutine(SalirDeInteraccion());
    }

    IEnumerator SalirDeInteraccion()
    {
        _ctx?.OnCloseStore();
        canvasGroupFade.gameObject.SetActive(true);

        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, 0.5f));
        yield return new WaitForSeconds(0.5f);

        if (objetoCamaraSecundaria != null)
            objetoCamaraSecundaria.SetActive(false);

        if (camaraSecundaria != null) camaraSecundaria.enabled = false;
        if (camaraPrincipal != null) camaraPrincipal.enabled = true;

        if (scriptMovimientoJugador != null)
            scriptMovimientoJugador.enabled = true;

        foreach (GameObject obj in objetosUIAOcultar)
            if (obj != null) obj.SetActive(true);

        panelBotonesIniciales?.SetActive(false);
        panelContenidoTienda?.SetActive(false);

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
