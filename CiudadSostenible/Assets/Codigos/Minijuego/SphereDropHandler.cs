using UnityEngine;

public class SphereDropHandler : MonoBehaviour
{
    /// Lógica para soltar una esfera sobre un bowl con layer "Bowl".
    public void DropItemAtMousePosition(InventorySlot parentSlot, Camera currentCamera, LayerMask bowlLayer, float maxDropDistance)
    {
        if (parentSlot == null || parentSlot.IsEmpty()) return;

        ItemData itemData = parentSlot.GetItemData();
        if (itemData == null || itemData.worldPrefab == null) return;

        // Lanzamos un raycast desde la cámara hacia donde está el mouse
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDropDistance, bowlLayer))
        {
            // Asegúrate de que el bowl tenga el componente BowlCapacity
            BowlCapacity bowl = hit.collider.GetComponent<BowlCapacity>();
            if (bowl != null && bowl.TryAddSphere())
            {
                // Instanciamos el objeto (esfera) levemente sobre el punto de impacto
                Vector3 spawnPosition = hit.point + Vector3.up * 0.1f;
                GameObject spawnedObject = Instantiate(itemData.worldPrefab, spawnPosition, Quaternion.identity);

                // Le asignamos el tag "Esfera" y le añadimos la lógica de cambio de prefab
                spawnedObject.tag = "Esfera";

                SwitchPrefabOnClick switcher = spawnedObject.AddComponent<SwitchPrefabOnClick>();
                switcher.Initialize(itemData, bowl);

                bowl.RegisterSphere(spawnedObject);
                parentSlot.RemoveQuantity(1);
            }
            else
            {
                Debug.Log("❌ Bowl lleno. No se agregó la esfera.");
            }
        }
        else
        {
            Debug.Log("⚠️ No se hizo hit en ningún objeto del layer 'Bowl'.");
        }
    }
}
