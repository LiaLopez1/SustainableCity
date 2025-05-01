using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BowlCapacity : MonoBehaviour
{
    [Header("Configuración")]
    public int maxCapacity = 2;
    public TextMeshProUGUI fullMessageText;
    public GameObject resultPrefab;

    private int currentCount = 0;
    private int alternatedCount = 0;

    public bool TryAddSphere()
    {
        if (currentCount >= maxCapacity)
            return false;

        currentCount++;

        if (currentCount == maxCapacity && fullMessageText != null)
        {
            fullMessageText.gameObject.SetActive(true);
            Invoke("HideMessage", 2f);
        }

        return true;
    }

    public void RegisterSphere(GameObject sphere)
    {
        // El registro ahora es opcional pero mantenido por compatibilidad
    }

    public void NotifyAlternateState(GameObject obj)
    {
        alternatedCount++;

        if (alternatedCount >= maxCapacity)
        {
            TransformAndSpawnResult();
        }
    }

    private void TransformAndSpawnResult()
    {
        // Destruir todas las esferas con el tag "Esfera"
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Esfera"))
        {
            Destroy(obj);
        }

        // Instanciar el resultado final en el centro del bowl
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
        Instantiate(resultPrefab, spawnPosition, Quaternion.identity);

        // Reset
        currentCount = 0;
        alternatedCount = 0;
    }

    private void HideMessage()
    {
        if (fullMessageText != null)
            fullMessageText.gameObject.SetActive(false);
    }

    private void OnCollisionExit(Collision collision)
    {
        currentCount = Mathf.Max(0, currentCount - 1);
    }
}
