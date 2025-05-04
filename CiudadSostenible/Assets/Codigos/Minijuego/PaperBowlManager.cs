using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PaperBowlManager : MonoBehaviour
{
    [Header("Configuración")]
    public string targetTag = "Paper";
    public int maxCapacity = 2;
    public TextMeshProUGUI mensajeTMP;

    [Header("Prefab al completar")]
    public GameObject resultadoFinalPrefab;
    public Transform resultadoSpawnPoint;

    [Header("Debug")]
    public int contadorPapelesEnBowl = 0;

    private List<GameObject> papeles = new List<GameObject>();
    private int papelesDestruidosAcumulados = 0;

    public bool EstaLleno()
    {
        return papeles.Count >= maxCapacity;
    }

    public bool HayObjetosDentro()
    {
        return papeles.Count > 0;
    }

    public List<GameObject> GetListaPapeles()
    {
        return new List<GameObject>(papeles);
    }

    public void MostrarMensajeFull()
    {
        if (mensajeTMP != null)
        {
            mensajeTMP.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HideTMPAfterSeconds(2f));
        }
    }

    public void DestruirPrimerPapel()
    {
        if (papeles.Count > 0)
        {
            GameObject papel = papeles[0];
            papeles.RemoveAt(0);
            contadorPapelesEnBowl = papeles.Count;

            if (papel != null)
                Destroy(papel);

            papelesDestruidosAcumulados++;

            if (papeles.Count == 0)
                PaperClickSplitter.posicionIndex = 0;

            if (papelesDestruidosAcumulados >= 2)
            {
                if (resultadoFinalPrefab != null && resultadoSpawnPoint != null)
                {
                    Instantiate(resultadoFinalPrefab, resultadoSpawnPoint.position, resultadoSpawnPoint.rotation);
                }

                papelesDestruidosAcumulados = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && !papeles.Contains(other.gameObject))
        {
            papeles.Add(other.gameObject);
            contadorPapelesEnBowl = papeles.Count;

            if (papeles.Count == maxCapacity)
            {
                MostrarMensajeFull();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag) && papeles.Contains(other.gameObject))
        {
            papeles.Remove(other.gameObject);
            contadorPapelesEnBowl = papeles.Count;

            if (papeles.Count == 0)
            {
                PaperClickSplitter.posicionIndex = 0;
            }
        }
    }

    private IEnumerator HideTMPAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (mensajeTMP != null)
        {
            mensajeTMP.gameObject.SetActive(false);
        }
    }
}
