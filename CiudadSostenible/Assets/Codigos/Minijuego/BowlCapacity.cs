using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;

public class BowlCapacity : MonoBehaviour
{
    [Header("Configuración")]
    public int maxCapacity = 5;

    [Header("UI")]
    public TextMeshProUGUI fullMessage;

    [Header("Prefab final a mostrar")]
    public GameObject finalPrefab;

    [Header("Sonidos")]
    public AudioClip addSphereSound;
    public AudioClip removeSphereSound;
    public AudioMixerGroup sfxMixerGroup;

    private AudioSource audioSource;

    private List<GameObject> currentSpheres = new List<GameObject>();

    private void Start()
    {
        if (fullMessage != null)
            fullMessage.gameObject.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }

    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public bool TryAddSphere()
    {
        if (currentSpheres.Count >= maxCapacity)
        {
            if (fullMessage != null)
                fullMessage.gameObject.SetActive(true);

            return false;
        }

        if (fullMessage != null)
            fullMessage.gameObject.SetActive(false);

        return true;
    }

    public void RegisterSphere(GameObject sphere)
    {
        if (!currentSpheres.Contains(sphere))
        {
            currentSpheres.Add(sphere);

            PlaySFX(addSphereSound);

            if (currentSpheres.Count >= maxCapacity && fullMessage != null)
                fullMessage.gameObject.SetActive(true);
        }
    }

    public void RemoveSphere(GameObject sphere)
    {
        if (currentSpheres.Contains(sphere))
        {
            currentSpheres.Remove(sphere);

            PlaySFX(removeSphereSound);

            if (currentSpheres.Count < maxCapacity && fullMessage != null)
                fullMessage.gameObject.SetActive(false);
        }
    }

    public void NotifyAlternateState()
    {
        int alternadas = 0;

        foreach (var sphere in currentSpheres)
        {
            if (sphere.name.Contains("Smashed") || sphere.name.Contains("Alternate"))
            {
                alternadas++;
            }
        }

        if (currentSpheres.Count == maxCapacity && alternadas == currentSpheres.Count)
        {
            foreach (var sphere in currentSpheres)
            {
                Destroy(sphere);
            }

            currentSpheres.Clear();

            Vector3 center = transform.position + Vector3.up * 0.5f;
            Instantiate(finalPrefab, center, Quaternion.identity);

            PlaySFX(removeSphereSound);


            if (fullMessage != null)
                fullMessage.gameObject.SetActive(false);
        }
    }


    public int GetCurrentCount() => currentSpheres.Count;
    public int GetMaxCapacity() => maxCapacity;
}
