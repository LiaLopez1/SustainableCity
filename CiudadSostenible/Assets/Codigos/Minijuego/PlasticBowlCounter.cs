
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

    public void ProcesarUnaBotella()
    {
        if (botellasQueue.Count == 0)
        {
            return;
        }

        GameObject botella = botellasQueue.Dequeue();
        if (botella != null)
        {
            ItemRecogible recogible = botella.GetComponent<ItemRecogible>();
            if (recogible != null && recogible.itemData != null && recogible.itemData.alternatePrefab != null)
            {
                Vector3 posicion = spawnPointProcesado.position;
                Quaternion rotacion = Quaternion.identity;
                Destroy(botella);
                Instantiate(recogible.itemData.alternatePrefab, posicion, rotacion);
                currentCount = Mathf.Max(0, currentCount - 1);
            }
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
