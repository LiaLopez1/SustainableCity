using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlasticBowlCounter : MonoBehaviour
{
    [Header("Configuración")]
    public string targetTag = "Botella";
    public int maxCapacity = 4;
    public TextMeshProUGUI mensajeTMP;
    public Transform spawnPointProcesado;
    public GameObject prefabProcesado; 

    [Header("Estado actual")]
    public int currentCount = 0;

    private Queue<GameObject> botellasQueue = new Queue<GameObject>();

    public bool IsFull()
    {
        return currentCount >= maxCapacity;
    }

    public void MostrarMensajeFull()
    {
        if (mensajeTMP != null)
        {
            mensajeTMP.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterSeconds(2f));
        }
    }


    public void ProcesarUnaBotellaDirecto()
    {
        // Solo procesar si hay al menos una botella
        if (currentCount <= 0) return;

        // Encontrar una botella con tag dentro del bowl
        GameObject[] botellas = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject botella in botellas)
        {
            if (GetComponent<Collider>().bounds.Contains(botella.transform.position))
            {
                Destroy(botella);
                currentCount = Mathf.Max(0, currentCount - 1);
                break;
            }
        }

        // Instanciar prefab fijo
        if (prefabProcesado != null && spawnPointProcesado != null)
        {
            Instantiate(prefabProcesado, spawnPointProcesado.position, Quaternion.identity);
            Debug.Log("✅ Botella procesada y objeto instanciado.");
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            currentCount++;
            botellasQueue.Enqueue(other.gameObject);

            if (currentCount >= maxCapacity && mensajeTMP != null)
            {
                mensajeTMP.gameObject.SetActive(true);
                StartCoroutine(HideMessageAfterSeconds(2f));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            currentCount = Mathf.Max(0, currentCount - 1);

            if (currentCount == 0)
            {
                BottleClickHandler.posicionIndex = 0;
            }
        }
    }

    private System.Collections.IEnumerator HideMessageAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (mensajeTMP != null)
        {
            mensajeTMP.gameObject.SetActive(false);
        }
    }
}
