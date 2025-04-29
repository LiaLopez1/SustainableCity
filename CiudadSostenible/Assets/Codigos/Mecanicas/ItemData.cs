using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo Item", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab;  // Prefab 3D para cuando se suelta
    [TextArea] public string description;
    public int maxStack = 10; 
}