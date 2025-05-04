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
            RecogerConTecla();
        }
    }

    private void OnMouseDown()
    {
        // Recoger con clic solo si el tag es PlasticStrips
        if (CompareTag("PlasticStrips"))
        {
            InventorySystem inv = FindObjectOfType<InventorySystem>();
            if (inv != null && itemData != null)
            {
                bool recogido = inv.AddItem(itemData);
                if (recogido)
                {
                    Debug.Log("✅ Recogido con clic: " + itemData.itemName);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("❌ Inventario lleno.");
                }
            }
        }
    }

    private void RecogerConTecla()
    {
        if (inventario != null && itemData != null)
        {
            bool recogido = inventario.AddItem(itemData);
            if (recogido)
            {
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
                uiRecolectar.MostrarMensaje("");
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
