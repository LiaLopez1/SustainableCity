using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class PantallaCarga : MonoBehaviour
{
    [Header("GameObjects de Contenido")]
    public GameObject[] pantallasContenido;

    [Header("Canvas de Cinemática")]
    public GameObject cinematicCanvas;
    public GameObject[] cinematicElements;

    [Header("Configuración")]
    public float totalCarga = 10f;
    public float tiempoPorPantalla = 3f;
    public float fadeDuration = 0.5f;
    public float tiempoCinematica = 2f;
    public Slider progresoSlider;
    public float timeCarta = 5f;
    public int escenaACargar = 1;

    private int indiceActual = 0;
    private CanvasGroup[] canvasGroups;
    private CanvasGroup[] cinematicCanvasGroups;
    private float tiempotranscurido = 0f;
    private bool cargaCompleta;
    private bool cinematicEnProgreso = false;

    void Start()
    {
        // Inicializar todos los CanvasGroups
        canvasGroups = new CanvasGroup[pantallasContenido.Length];
        for (int i = 0; i < pantallasContenido.Length; i++)
        {
            if (pantallasContenido[i] != null)
            {
                canvasGroups[i] = pantallasContenido[i].GetComponent<CanvasGroup>();
                if (canvasGroups[i] == null)
                {
                    canvasGroups[i] = pantallasContenido[i].AddComponent<CanvasGroup>();
                }

                pantallasContenido[i].SetActive(i == 0);
                canvasGroups[i].alpha = i == 0 ? 1f : 0f;
            }
        }

        if (progresoSlider != null)
        {
            progresoSlider.minValue = 0f;
            progresoSlider.maxValue = 1f;
            progresoSlider.value = 0f;
            progresoSlider.interactable = false;
        }

        InitializeCinematicCanvas();
    }

    void Update()
    {
        if (cinematicEnProgreso || cargaCompleta || IsTransitioning())
            return;

        // Ignorar clics si hay una transición en progreso
        if (Input.GetMouseButtonDown(0) && IsTransitioning())
        {
            return;
        }
    }

    private bool IsTransitioning()
    {
        return tiempotranscurido > 0f && tiempotranscurido < totalCarga;
    }

    void InitializeCinematicCanvas()
    {
        if (cinematicCanvas != null)
        {
            cinematicCanvas.SetActive(false);

            if (cinematicElements != null && cinematicElements.Length > 0)
            {
                cinematicCanvasGroups = new CanvasGroup[cinematicElements.Length];

                for (int i = 0; i < cinematicElements.Length; i++)
                {
                    if (cinematicElements[i] != null)
                    {
                        cinematicCanvasGroups[i] = cinematicElements[i].GetComponent<CanvasGroup>();
                        if (cinematicCanvasGroups[i] == null)
                        {
                            cinematicCanvasGroups[i] = cinematicElements[i].AddComponent<CanvasGroup>();
                        }

                        cinematicElements[i].SetActive(false);
                        cinematicCanvasGroups[i].alpha = 0f;
                        cinematicCanvasGroups[i].interactable = false;
                        cinematicCanvasGroups[i].blocksRaycasts = false;
                    }
                }
            }
        }
    }

    public void IniciarSecuenciaCarga(int escenacarga)
    {
        if (pantallasContenido == null || pantallasContenido.Length == 0)
        {
            SceneManager.LoadScene(escenacarga);
            return;
        }

        StartCoroutine(SecuenciaDeCarga(escenacarga));
    }

    private IEnumerator SecuenciaDeCarga(int escenacarga)
    {
        yield return StartCoroutine(MostrarPantalla(0));

        while (!cargaCompleta)
        {
            tiempotranscurido += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempotranscurido / totalCarga);

            if (progresoSlider != null)
            {
                progresoSlider.value = progreso;
            }

            if (tiempotranscurido >= (indiceActual + 1) * tiempoPorPantalla &&
                indiceActual < pantallasContenido.Length - 1)
            {
                yield return StartCoroutine(OcultarPantalla(indiceActual));
                indiceActual++;
                yield return StartCoroutine(MostrarPantalla(indiceActual));
            }

            if (tiempotranscurido >= totalCarga)
            {
                cargaCompleta = true;
            }

            yield return null;
        }

        yield return StartCoroutine(ShowCinematicAndLoad());
    }

    private IEnumerator ShowCinematicAndLoad()
    {
        cinematicEnProgreso = true;
        // Ocultar pantalla de carga
        if (pantallasContenido.Length > 0)
        {
            yield return StartCoroutine(OcultarPantalla(indiceActual));
        }

        // Mostrar canvas de cinemática
        if (cinematicCanvas != null)
        {
            cinematicCanvas.SetActive(true);

            // Mostrar elementos de cinemática uno por uno
            for (int i = 0; i < cinematicElements.Length; i++)
            {
                if (cinematicElements[i] != null)
                {
                    yield return StartCoroutine(MostrarElementoCinematica(i));

                    // Duración personalizada si es el segundo elemento
                    float tiempoEspera = (i == 1) ? timeCarta : tiempoCinematica;
                    yield return new WaitForSeconds(tiempoEspera);

                    if (i < cinematicElements.Length - 1) // No ocultar el último
                    {
                        yield return StartCoroutine(OcultarElementoCinematica(i));
                    }
                }
            }
        }

        // Cargar escena después de la cinemática
        SceneManager.LoadScene(escenaACargar);
    }

    public void SkipEscena()
    {
        SceneManager.LoadScene(escenaACargar);
    }

    private IEnumerator MostrarElementoCinematica(int index)
    {
        cinematicElements[index].SetActive(true);

        float tiempo = 0f;
        while (tiempo < fadeDuration)
        {
            tiempo += Time.deltaTime;
            cinematicCanvasGroups[index].alpha = Mathf.Lerp(0f, 1f, tiempo / fadeDuration);
            yield return null;
        }
        cinematicCanvasGroups[index].alpha = 1f;

        TypewriterEffect[] typewriters = cinematicElements[index].GetComponentsInChildren<TypewriterEffect>();
        foreach (var tw in typewriters)
        {
            if (tw != null) tw.RestartAnimation();
        }

        float tiempoTexto = 0f;
        if (typewriters.Length > 0)
        {
            foreach (var tw in typewriters)
            {
                float tiempoEstimado = tw.startDelay + (tw.GetComponent<TMP_Text>().text.Length / tw.charsPerSecond);
                if (tiempoEstimado > tiempoTexto) tiempoTexto = tiempoEstimado;
            }
        }

        yield return new WaitForSeconds(tiempoCinematica + tiempoTexto);
    }

    private IEnumerator OcultarElementoCinematica(int index)
    {
        float tiempo = 0f;

        while (tiempo < fadeDuration)
        {
            tiempo += Time.deltaTime;
            cinematicCanvasGroups[index].alpha = Mathf.Lerp(1f, 0f, tiempo / fadeDuration);
            yield return null;
        }

        cinematicCanvasGroups[index].alpha = 0f;
        cinematicElements[index].SetActive(false);
    }

    private IEnumerator MostrarPantalla(int indice)
    {
        if (indice < 0 || indice >= pantallasContenido.Length || pantallasContenido[indice] == null)
            yield break;

        // Deshabilitar interacción
        canvasGroups[indice].interactable = false;
        canvasGroups[indice].blocksRaycasts = false;

        pantallasContenido[indice].SetActive(true);
        CanvasGroup cg = canvasGroups[indice];

        float tiempo = 0f;
        while (tiempo < fadeDuration)
        {
            tiempo += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, tiempo / fadeDuration);
            yield return null;
        }

        cg.alpha = 1f;

        // Habilitar interacción solo cuando termine
        //canvasGroups[indice].interactable = true;
        //canvasGroups[indice].blocksRaycasts = true;
    }

    private IEnumerator OcultarPantalla(int indice)
    {
        if (indice < 0 || indice >= pantallasContenido.Length || pantallasContenido[indice] == null)
            yield break;

        CanvasGroup cg = canvasGroups[indice];

        float tiempo = 0f;
        while (tiempo < fadeDuration)
        {
            tiempo += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, tiempo / fadeDuration);
            yield return null;
        }

        cg.alpha = 0f;
        pantallasContenido[indice].SetActive(false);
    }
}