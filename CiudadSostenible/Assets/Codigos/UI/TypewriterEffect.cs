using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour
{
    [Header("Configuración")]
    public float charsPerSecond = 20f;
    public float startDelay = 0.5f;
    public AudioClip typeSound;
    [Range(0.1f, 1f)] public float soundVolume = 0.5f;

    private TMP_Text textComponent;
    private string fullText;
    private AudioSource audioSource;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        fullText = textComponent.text;
        textComponent.text = "";

        if (typeSound != null)
        {
            // Configuración optimizada del AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
            audioSource.loop = false;
            audioSource.priority = 0; // Máxima prioridad
            audioSource.spatialBlend = 0f; // 2D completo
            audioSource.bypassEffects = true;
            audioSource.bypassListenerEffects = true;
            audioSource.bypassReverbZones = true;
        }
    }

    void Start()
    {
        StartTyping();
    }

    public void StartTyping()
    {
        if (isTyping) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        textComponent.text = "";

        // Precargar el sonido antes del delay
        if (typeSound != null)
        {
            audioSource.clip = typeSound;
            audioSource.Play();
            audioSource.Pause();
        }

        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i <= fullText.Length; i++)
        {
            textComponent.text = fullText.Substring(0, i);

            if (typeSound != null && i < fullText.Length && !char.IsWhiteSpace(fullText[i]))
            {
                // Reproducción optimizada del sonido
                audioSource.UnPause();
                audioSource.time = 0f; // Reiniciar el sonido
                audioSource.Play();

                // Pausar inmediatamente para preparar el próximo sonido
                audioSource.Pause();
            }

            yield return new WaitForSeconds(1f / charsPerSecond);
        }

        isTyping = false;
    }

    public void RestartAnimation()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText());
    }
}