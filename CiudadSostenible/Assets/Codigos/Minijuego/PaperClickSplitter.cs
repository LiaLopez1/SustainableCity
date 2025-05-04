using UnityEngine;
using System;

public class PaperClickSplitter : MonoBehaviour
{
    private int currentStep = 0;
    private Transform[] tiras;
    private float[] desplazamientos = { 0.20f, 0.13f, 0.06f };

    [Header("Spawn Final")]
    public Transform paperFinalSpawnPoint;
    public Action onPaperCompletado;

    private static int posicionIndex = 0;
    private const int maxPorFila = 7;

    private bool yaMovido = false;
    private bool yaProcesado = false;

    private ItemData itemData;

    void Start()
    {
        tiras = new Transform[3];
        tiras[0] = transform.Find("Tira4");
        tiras[1] = transform.Find("Tira3");
        tiras[2] = transform.Find("Tira2");

        itemData = GetComponent<ItemRecogible>()?.itemData;
    }

    void OnMouseDown()
    {
        if (currentStep < tiras.Length)
        {
            Transform tira = tiras[currentStep];
            if (tira != null)
            {
                float offset = desplazamientos[currentStep];
                tira.position += Vector3.forward * offset;
            }
            currentStep++;
        }
        else if (!yaMovido && paperFinalSpawnPoint != null)
        {
            if (posicionIndex >= maxPorFila)
                posicionIndex = 0;

            Vector3 offset = new Vector3(-0.1f * posicionIndex, 0f, 0f);
            transform.position = paperFinalSpawnPoint.position + offset;
            posicionIndex++;

            yaMovido = true;
            onPaperCompletado?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaProcesado) return;

        // Detecta colisión con objeto del layer WaterBowl
        if (other.gameObject.layer == LayerMask.NameToLayer("WaterBowl"))
        {
            if (itemData != null && itemData.alternatePrefab != null)
            {
                Instantiate(itemData.alternatePrefab, transform.position, transform.rotation);
                Destroy(gameObject);
                yaProcesado = true;
                Debug.Log("🧻 Papel convertido tras contacto con agua.");
            }
        }
    }
}
