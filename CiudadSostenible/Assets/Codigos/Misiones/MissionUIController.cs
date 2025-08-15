using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionUIController : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelMision;
    public GameObject imageExclamacion;
    public TextMeshProUGUI textoTituloMision;
    public TextMeshProUGUI textoExplicacion;
    public Button botonCerrar;

    [Header("Animación campana (sin Animator)")]
    [SerializeField] private RectTransform bellRect;     // <- arrastra aquí el RectTransform de imageExclamacion
    [SerializeField] private float popScaleUp = 1.18f;   // cuánto “crece” (1.0 = sin cambio)
    [SerializeField] private float popTimeUp = 0.12f;    // tiempo de subida
    [SerializeField] private float popTimeDown = 0.12f;  // tiempo de bajada

    private MissionManager missionManager;
    private Coroutine bellRoutine;
    private bool exclamEstaVisible = false;  // para detectar el cambio de estado (off -> on)

    void Start()
    {
        // Buscar el MissionManager en la escena
        missionManager = FindObjectOfType<MissionManager>();

        // Asegurarse de que el panel está oculto al inicio
        panelMision.SetActive(false);
        botonCerrar.onClick.AddListener(CerrarPanel);

        // Si no asignaste bellRect, intenta tomarlo de imageExclamacion automáticamente
        if (bellRect == null && imageExclamacion != null)
            bellRect = imageExclamacion.GetComponent<RectTransform>();
    }

    public void MostrarPanelMision()
    {
        if (missionManager == null) return;

        var mision = missionManager.ObtenerMisionActual();
        if (mision == null) return;

        textoTituloMision.text = mision.nombreMision;
        textoExplicacion.text = mision.descripcion;

        panelMision.SetActive(true);

        // Al abrir el panel, ocultamos el ícono y marcamos la misión como mostrada
        imageExclamacion.SetActive(false);
        exclamEstaVisible = false;

        mision.fueMostradaAlJugador = true;

        // Refresca el HUD de la misión (título/progreso)
        missionManager.ActualizarTextoHUD();
    }

    public void CerrarPanel()
    {
        panelMision.SetActive(false);
    }

    // Se llama desde MissionManager cuando cambia/avanza la misión
    public void ActualizarExclamacion()
    {
        if (missionManager == null)
        {
            missionManager = FindObjectOfType<MissionManager>();
            if (missionManager == null) return;
        }

        var mision = missionManager.ObtenerMisionActual();
        bool debeMostrarse = (mision != null && !mision.fueMostradaAlJugador);

        // borde ascendente: pasa de oculto -> visible
        if (debeMostrarse && !exclamEstaVisible)
        {
            imageExclamacion.SetActive(true);
            exclamEstaVisible = true;
            var pop = imageExclamacion.GetComponent<BellPopPlayer>();
            if (pop != null) pop.Play();
        }
        // borde descendente: visible -> oculto
        else if (!debeMostrarse && exclamEstaVisible)
        {
            imageExclamacion.SetActive(false);
            exclamEstaVisible = false;
        }
        // si no cambia el estado, no hacemos nada (evita repetir el pop)
    }

    // -------- Animación por código (pop de escala) --------
    private void PlayBellPop()
    {
        if (bellRect == null) return;

        // Si ya hay una animación corriendo, la reiniciamos
        if (bellRoutine != null) StopCoroutine(bellRoutine);
        bellRoutine = StartCoroutine(BellPopRoutine());
    }

    private System.Collections.IEnumerator BellPopRoutine()
    {
        Vector3 baseScale = Vector3.one;
        Vector3 targetScale = Vector3.one * Mathf.Max(1f, popScaleUp);

        // subida
        float t = 0f;
        while (t < popTimeUp)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / popTimeUp);
            // Easing suave
            float e = k * k * (3f - 2f * k); // SmoothStep
            bellRect.localScale = Vector3.LerpUnclamped(baseScale, targetScale, e);
            yield return null;
        }

        // bajada
        t = 0f;
        while (t < popTimeDown)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / popTimeDown);
            float e = k * k * (3f - 2f * k);
            bellRect.localScale = Vector3.LerpUnclamped(targetScale, baseScale, e);
            yield return null;
        }

        bellRect.localScale = baseScale;
        bellRoutine = null;
    }
}
