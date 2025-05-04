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

    private bool yaMovido = false;
    public static int posicionIndex = 0;

    private PaperBowlManager bowlManager; // ✅ Se detectará automáticamente

    void Start()
    {
        tiras = new Transform[3];
        tiras[0] = transform.Find("Tira4");
        tiras[1] = transform.Find("Tira3");
        tiras[2] = transform.Find("Tira2");

        // 🔎 Detectar automáticamente el bowl cercano por Layer
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f); // ajusta el radio si es necesario
        foreach (Collider col in colliders)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("WaterBowl"))
            {
                bowlManager = col.GetComponent<PaperBowlManager>();
                if (bowlManager != null)
                {
                    break;
                }
            }
        }
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
            if (bowlManager != null && bowlManager.EstaLleno())
            {
                bowlManager.MostrarMensajeFull();
                return;
            }

            Vector3 offset = new Vector3(-0.1f * posicionIndex, 0f, 0f);
            transform.position = paperFinalSpawnPoint.position + offset;
            posicionIndex++;

            yaMovido = true;
            onPaperCompletado?.Invoke();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (yaMovido && other.gameObject.layer == LayerMask.NameToLayer("WaterBowl"))
        {
            ItemRecogible recogible = GetComponent<ItemRecogible>();
            if (recogible != null && recogible.itemData != null && recogible.itemData.alternatePrefab != null)
            {
                Instantiate(recogible.itemData.alternatePrefab, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}
