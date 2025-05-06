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

    [Header("Control de entrada")]
    public bool clicsPermitidos = false; // ← NUEVO

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
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
            audioSource.loop = false;
            audioSource.priority = 0;
            audioSource.spatialBlend = 0f;
            audioSource.bypassEffects = true;
            audioSource.bypassListenerEffects = true;
            audioSource.bypassReverbZones = true;
        }
    }

    void Start()
    {
        StartTyping();
    }

    void Update()
    {
        if (clicsPermitidos && isTyping && Input.GetMouseButtonDown(0))
        {
            SkipToEnd();
        }
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
        if (typeSound != null && audioSource != null)
        {
            audioSource.clip = typeSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < fullText.Length; i++)
        {
            textComponent.text += fullText[i];

            yield return new WaitForSeconds(1f / charsPerSecond);
        }

        isTyping = false;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void RestartAnimation()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        textComponent.text = "";
        isTyping = true;
        typingCoroutine = StartCoroutine(TypeText());
    }

    public void SkipToEnd()
    {
        StopAllCoroutines();
        textComponent.text = fullText;
        isTyping = false;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
