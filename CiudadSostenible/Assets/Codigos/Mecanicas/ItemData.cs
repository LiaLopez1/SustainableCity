using UnityEngine; // ¡Este using es esencial!

[CreateAssetMenu(fileName = "Nuevo Item", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab;
    public GameObject alternatePrefab; // Nuevo prefab para el cambio
    public int maxStack = 10;
}