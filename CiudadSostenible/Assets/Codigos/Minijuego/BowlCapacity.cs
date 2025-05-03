using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BowlCapacity : MonoBehaviour
{
    [Header("Configuración")]
    public int maxCapacity = 5;

    [Header("UI")]
    public TextMeshProUGUI fullMessage;

    [Header("Prefab final a mostrar")]
    public GameObject finalPrefab;

    private List<GameObject> currentSpheres = new List<GameObject>();

    private void Start()
    {
        if (fullMessage != null)
            fullMessage.gameObject.SetActive(false);
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

            if (currentSpheres.Count >= maxCapacity && fullMessage != null)
                fullMessage.gameObject.SetActive(true);
        }
    }

    public void RemoveSphere(GameObject sphere)
    {
        if (currentSpheres.Contains(sphere))
        {
            currentSpheres.Remove(sphere);

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

        if (alternadas == currentSpheres.Count && currentSpheres.Count > 0)
        {

            // Destruir todas las esferas
            foreach (var sphere in currentSpheres)
            {
                Destroy(sphere);
            }

            currentSpheres.Clear();

            // Instanciar el prefab final
            Vector3 center = transform.position + Vector3.up * 0.5f;
            Instantiate(finalPrefab, center, Quaternion.identity);

            if (fullMessage != null)
                fullMessage.gameObject.SetActive(false);
        }
    }

    public int GetCurrentCount() => currentSpheres.Count;
    public int GetMaxCapacity() => maxCapacity;
}
