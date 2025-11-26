using System.Collections;
using UnityEngine;

public class MissionCutsceneController : MonoBehaviour
{
    [Header("Fade negro")]
    public CanvasGroup blackFadeCanvas;
    public float fadeDuration = 0.4f;
    public float cutsceneDuration = 3f;

    [Header("Cámaras")]
    public Camera mainCamera;
    public Camera cutsceneCamera;   // Déjala DESACTIVADA en la jerarquía al inicio

    [Header("Objeto (espejo) que se desactiva y queda apagado")]
    public GameObject espejoADesactivar;
    public float delayDesactivarEspejo = 1.5f;

    bool isPlaying = false;

    public void PlayCutscene()
    {
        if (!isPlaying && gameObject.activeInHierarchy)
            StartCoroutine(CutsceneRoutine());
    }

    IEnumerator CutsceneRoutine()
    {
        isPlaying = true;

        // Estado inicial de cámaras
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
            mainCamera.enabled = true;
        }

        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = false;
            cutsceneCamera.gameObject.SetActive(false);
        }

        // Canvas negro preparado
        if (blackFadeCanvas != null)
        {
            blackFadeCanvas.gameObject.SetActive(true);
            blackFadeCanvas.alpha = 0f;
        }

        // 🔹 En paralelo: apagar el espejo después de X segundos
        StartCoroutine(DesactivarEspejoConDelay());

        // ---------- FADE IN 1 (sobre la cámara ACTUAL) ----------
        yield return StartCoroutine(Fade(0f, 1f));

        // *** PANTALLA NEGRA ***
        // Cambiamos de cámara AQUÍ, cuando ya está todo tapado
        if (mainCamera != null)
            mainCamera.enabled = false;

        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(true);
            cutsceneCamera.enabled = true;
        }

        // ---------- FADE OUT 1 (se revela la cámara de cinemática) ----------
        yield return StartCoroutine(Fade(1f, 0f));

        // Se ve la cinemática durante X segundos
        yield return new WaitForSeconds(cutsceneDuration);

        // ---------- FADE IN 2 (desde la cinemática) ----------
        yield return StartCoroutine(Fade(0f, 1f));

        // *** PANTALLA NEGRA ***
        // Volvemos a la cámara principal
        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = false;
            cutsceneCamera.gameObject.SetActive(false);
        }

        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
            mainCamera.enabled = true;
        }

        // ---------- FADE OUT 2 (ya de vuelta al juego) ----------
        yield return StartCoroutine(Fade(1f, 0f));

        if (blackFadeCanvas != null)
            blackFadeCanvas.gameObject.SetActive(false);

        // El espejo NO se reactiva nunca: queda apagado.

        isPlaying = false;
    }

    IEnumerator DesactivarEspejoConDelay()
    {
        if (espejoADesactivar == null)
            yield break;

        yield return new WaitForSeconds(delayDesactivarEspejo);

        espejoADesactivar.SetActive(false);  // Se apaga y no se vuelve a encender
    }

    IEnumerator Fade(float from, float to)
    {
        if (blackFadeCanvas == null) yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            blackFadeCanvas.alpha = Mathf.Lerp(from, to, normalized);
            yield return null;
        }

        blackFadeCanvas.alpha = to;
    }
}
