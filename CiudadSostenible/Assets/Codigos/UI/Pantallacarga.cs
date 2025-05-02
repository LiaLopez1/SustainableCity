using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PantallaCarga : MonoBehaviour
{
    [Header("GameObjects de Contenido")]
    public GameObject[] pantallasContenido;

    [Header("Configuración")]
    public float totalCarga = 10f;
    public float tiempoPorPantalla = 3f;
    public float fadeDuration = 0.5f;
    public Slider progresoSlider;

    private int indiceActual = 0;
    private CanvasGroup[] canvasGroups;
    private float tiempotranscurido = 0f;
    private bool cargaCompleta;

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
        }
    }

    public void IniciarSecuenciaCarga(int escenacarga)
    {
        if (pantallasContenido.Length == 0)
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

            if(tiempotranscurido >= (indiceActual + 1) * tiempoPorPantalla &&
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

        SceneManager.LoadScene(escenacarga);
    }

    private IEnumerator MostrarPantalla(int indice)
    {
        if (indice < 0 || indice >= pantallasContenido.Length || pantallasContenido[indice] == null)
            yield break;

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