using UnityEngine;

public class SwitchPrefabOnClick : MonoBehaviour
{
    private ItemData itemData; // Referencia al ItemData original
    private bool isAlternate = false;

    // Llama a este m�todo al instanciar la esfera
    public void Initialize(ItemData data)
    {
        itemData = data;
    }

    private void OnMouseDown() 
    {
        if (itemData == null || itemData.alternatePrefab == null) return;

        // Cambia entre prefabs
        GameObject newPrefab = isAlternate ? itemData.worldPrefab : itemData.alternatePrefab;
        Instantiate(newPrefab, transform.position, transform.rotation);
        Destroy(gameObject); 
        isAlternate = !isAlternate; // Alterna el estado
    }
}