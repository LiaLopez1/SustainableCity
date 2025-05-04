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

    [Header("Debug")]
    public int contadorPapelesEnBowl = 0; // 🔹 contador visible

    private List<GameObject> papeles = new List<GameObject>();

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
