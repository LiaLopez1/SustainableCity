using UnityEngine;

public class SwitchPrefabOnClick : MonoBehaviour
{
    private ItemData itemData; // Referencia al ItemData original
    private bool isAlternate = false;

    // Llama a este método al instanciar la esfera
    public void Initialize(ItemData data)
    {
        itemData = data;
    }

    private void OnMouseDown() // Se ejecuta al hacer clic con el mouse
    {
        if (itemData == null || itemData.alternatePrefab == null) return;

        // Cambia entre prefabs
        GameObject newPrefab = isAlternate ? itemData.worldPrefab : itemData.alternatePrefab;
        Instantiate(newPrefab, transform.position, transform.rotation);
        Destroy(gameObject); // Destruye el objeto actual
        isAlternate = !isAlternate; // Alterna el estado
    }
}