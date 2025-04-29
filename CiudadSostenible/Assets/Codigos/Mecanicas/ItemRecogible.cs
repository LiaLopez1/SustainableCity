using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    public ItemData itemData;

    private UIRecolectarBasura uiRecolectar;
    private bool jugadorCerca = false;
    private InventorySystem inventario;

    void Start()
    {
        uiRecolectar = FindObjectOfType<UIRecolectarBasura>();
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            if (inventario != null && itemData != null)
            {
                inventario.AddItem(itemData);

                BasuraSpawner spawner = FindObjectOfType<BasuraSpawner>();
                if (spawner != null)
                {
                    spawner.RecogerBasura(itemData.itemName);
                }

                if (uiRecolectar != null)
                {
                    uiRecolectar.OcultarMensaje();
                }

                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            inventario = other.GetComponent<InventorySystem>();

            if (uiRecolectar != null)
            {
                uiRecolectar.MostrarMensaje("Recoger");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            inventario = null;

            if (uiRecolectar != null)
            {
                uiRecolectar.OcultarMensaje();
            }
        }
    }
}