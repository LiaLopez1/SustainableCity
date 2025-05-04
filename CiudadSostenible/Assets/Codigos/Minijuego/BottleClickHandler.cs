using UnityEngine;
using System;

public class BottleClickHandler : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform tapaSpawnPoint;
    public Transform bottleFinalSpawnPoint;

    [Header("Configuración automática")]
    private PlasticBowlCounter bowlCounter;

    public Action onBotellaCompletada;

    private bool tapaSeparada = false;
    private bool botellaMovida = false;

    public static int posicionIndex = 0;
    private const int maxPorFila = 7;

    private void OnMouseDown()
    {
        if (!tapaSeparada)
        {
            Transform cap = transform.Find("BottleCap");
            if (cap != null && tapaSpawnPoint != null)
            {
                cap.position = tapaSpawnPoint.position;

                if (cap.GetComponent<Collider>() == null)
                    cap.gameObject.AddComponent<BoxCollider>();

                Rigidbody rb = cap.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = cap.gameObject.AddComponent<Rigidbody>();

                rb.useGravity = true;
                rb.isKinematic = false;

                tapaSeparada = true;
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
                if (bowlCounter == null)
                {
                    bowlCounter = FindObjectOfType<PlasticBowlCounter>();
                }

                if (bowlCounter != null && bowlCounter.IsFull())
                {
                    bowlCounter.MostrarMensajeFull();
                    return;
                }

                // Reiniciar la fila si ya hay 7 botellas
                if (posicionIndex >= maxPorFila)
                {
                    posicionIndex = 0;
                }

                Transform cuerpo = transform.Find("Bottle");
                if (cuerpo != null)
                {
                    Vector3 offset = new Vector3(-0.2f * posicionIndex, 0f, 0f);
                    Vector3 nuevaPosicion = bottleFinalSpawnPoint.position + offset;
                    cuerpo.position = nuevaPosicion;

                    botellaMovida = true;
                    posicionIndex++;
                    onBotellaCompletada?.Invoke();
                }
                else
                {
                    Debug.LogWarning("❌ No se encontró 'Bottle'.");
                }
            }
        }
    }
}
