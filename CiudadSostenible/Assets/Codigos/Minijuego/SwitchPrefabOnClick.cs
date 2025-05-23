﻿using UnityEngine;

public class SwitchPrefabOnClick : MonoBehaviour
{
    private ItemData itemData;
    private BowlCapacity bowlRef;
    private bool isAlternate = false;

    public void Initialize(ItemData data, BowlCapacity bowl)
    {
        itemData = data;
        bowlRef = bowl;
    }

    private void OnMouseDown()
    {
        if (itemData == null || itemData.alternatePrefab == null) return;

        GameObject newPrefab = isAlternate ? itemData.worldPrefab : itemData.alternatePrefab;
        GameObject newObj = Instantiate(newPrefab, transform.position, transform.rotation);

        // Conserva el mismo tag
        newObj.tag = gameObject.tag;

        if (bowlRef != null)
        {
            bowlRef.RemoveSphere(gameObject);
            bowlRef.RegisterSphere(newObj);

            if (!isAlternate)
                bowlRef.NotifyAlternateState(); // ✅ restaurado
        }

        Destroy(gameObject);
        isAlternate = !isAlternate;
    }
}
