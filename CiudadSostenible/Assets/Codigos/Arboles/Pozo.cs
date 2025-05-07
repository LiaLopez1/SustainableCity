using UnityEngine;

public class Pozo : MonoBehaviour
{
    private bool jugadorDentro = false;
    public GameObject textoInteraccion;

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
            if (item != null && item.itemName.ToLower().Contains("bidon"))
            {
                BidonDeAguaManager.Instance.LlenarBidon(item);
                Debug.Log("¡Bidón llenado!");
                return;
            }
        }

        Debug.Log("No tienes bidón para llenar.");
    }
}
