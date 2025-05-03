using UnityEngine;
using System;

public class BottleClickHandler : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform tapaSpawnPoint;
    public Transform bottleFinalSpawnPoint;

    public Action onBotellaCompletada;

    private bool tapaSeparada = false;
    private bool botellaMovida = false;

    private void OnMouseDown()
    {
        if (!tapaSeparada)
        {
            Transform cap = transform.Find("BottleCap");
            if (cap != null && tapaSpawnPoint != null)
            {
                cap.position = tapaSpawnPoint.position;

                if (cap.GetComponent<Collider>() == null)
                {
                    cap.gameObject.AddComponent<BoxCollider>();
                }

                Rigidbody rb = cap.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = cap.gameObject.AddComponent<Rigidbody>();
                }

                rb.useGravity = true;
                rb.isKinematic = false;

                tapaSeparada = true;
                Debug.Log("🧴 Tapa separada y física activada.");
            }
            else
            {
                Debug.LogWarning("❌ No se encontró 'BottleCap' o falta el spawn.");
            }
        }
        else if (!botellaMovida)
        {
            if (bottleFinalSpawnPoint != null)
            {
                Transform cuerpo = transform.Find("Bottle");
                if (cuerpo != null)
                {
                    cuerpo.position = bottleFinalSpawnPoint.position;
                    botellaMovida = true;
                    onBotellaCompletada?.Invoke();
                    Debug.Log("📦 Cuerpo de botella movido al tercer spawn.");
                }
                else
                {
                    Debug.LogWarning("❌ No se encontró 'Bottle'.");
                }
            }
        }
    }
}
