using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    public Sprite icono; // Asigna el icono en el Inspector
    public string tipoItem; // Ej: "Plastico", "Papel", "Organico"

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            InventorySystem inventario = other.GetComponent<InventorySystem>();
            if (inventario != null)
            {
                inventario.AddItem(tipoItem, 1, icono);
                Destroy(gameObject); // Elimina el objeto de la escena
            }
        }
    }
}