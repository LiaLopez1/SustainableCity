using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Audio;

public class UISoundManager : MonoBehaviour
{
    [Header("Sonidos UI")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    [Range(0.1f, 1f)] public float volume = 0.8f;

    [Header("Configuración de Audio")]
    public AudioMixerGroup sfxMixerGroup;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
        else
        {
            Debug.LogWarning("No se asignó un AudioMixerGroup, los sonidos UI funcionarán pero sin control de volumen por mixer");
        }

        audioSource.playOnAwake = false;
    }

    void Start()
    {
        RegisterAllButtons();
    }

    void RegisterAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button button in allButtons)
        {
            RegisterButton(button);
        }
    }

    void RegisterButton(Button button)
    {
        button.onClick.RemoveAllListeners();

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ??
                             button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnUIHover(); });
        trigger.triggers.Add(entry);

        button.onClick.AddListener(OnUIClick);
    }

    void OnUIHover()
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound, volume);
        }
    }

    void OnUIClick()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
}