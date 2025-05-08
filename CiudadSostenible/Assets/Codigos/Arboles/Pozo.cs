using UnityEngine;
using System.Collections;
using TMPro; // Asegúrate de tener esto arriba

public class Pozo : MonoBehaviour
{
    private bool jugadorDentro = false;
    public GameObject textoInteraccion;
    public TextMeshProUGUI textoFeedback;
    private Coroutine feedbackCoroutine;
    public ItemData bidonVacioItem;
    public ItemData bidonLlenoItem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;
            textoInteraccion?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            textoInteraccion?.SetActive(false);
        }
    }

    private void Update()
    {
        if (jugadorDentro && Input.GetKeyDown(KeyCode.E))
        {
            LlenarPrimerBidonDisponible();
        }
    }

    private void LlenarPrimerBidonDisponible()
    {
        var inventario = FindObjectOfType<InventorySystem>();
        if (inventario == null) return;

        foreach (var slot in inventario.slots)
        {
            ItemData item = slot.GetItemData();
            if (item == bidonVacioItem)
            {
                if (inventario.RemoveItem(bidonVacioItem, 1))
                {
                    inventario.AddItem(bidonLlenoItem, 1);
                    MostrarFeedback("Water jug filled!");
                    return;
                }
            }
        }

        MostrarFeedback("You need an empty water jug to use the well.");
    }


    void MostrarFeedback(string mensaje)
    {
        if (textoFeedback == null) return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        textoFeedback.text = mensaje;
        textoFeedback.gameObject.SetActive(true);
        feedbackCoroutine = StartCoroutine(EsconderFeedback());
    }

    IEnumerator EsconderFeedback()
    {
        yield return new WaitForSeconds(2.5f);
        textoFeedback.gameObject.SetActive(false);
    }

}
