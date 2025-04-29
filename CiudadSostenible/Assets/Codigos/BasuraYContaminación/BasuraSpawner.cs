using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TipoBasura
{
    public string nombreTipo; // "Papel", "Plástico", etc.
    public GameObject prefab; // Prefab de esa basura
    [Range(0f, 100f)]
    public float probabilidad; // Probabilidad individual (0 a 100%)
}

public class BasuraSpawner : MonoBehaviour
{
    [Header("Tipos de Basura")]
    public List<TipoBasura> tiposBasura; // Lista de tipos de basura

    [Header("Configuración de Spawn")]
    public float alturaBasura = 0.133f;
    public float rangoMaximoX = 40f;
    public float rangoMaximoZ = 40f;
    public int cantidadMaximaBasura = 50;
    public float tiempoTotalGeneracion = 60f;

    [Header("UI")]
    public Slider sliderSuciedad;
    public TMP_Text textoContadoresBasura;

    [Header("Contadores de Basura")]
    public int contadorPapel = 0;
    public int contadorPlastico = 0;
    public int contadorOrganico = 0;
    public int contadorVidrio = 0;

    private int basuraActual = 0;
    private float tiempoEntreSpawns;
    private float tiempoSiguienteSpawn;

    void Start()
    {
        if (cantidadMaximaBasura > 0 && tiempoTotalGeneracion > 0)
        {
            tiempoEntreSpawns = tiempoTotalGeneracion / cantidadMaximaBasura;
            tiempoSiguienteSpawn = Time.time + tiempoEntreSpawns;
        }
        else
        {
            Debug.LogError("Configura cantidadMaximaBasura y tiempoTotalGeneracion correctamente.");
        }

        ActualizarUI();
    }

    void Update()
    {
        if (Time.time >= tiempoSiguienteSpawn && basuraActual < cantidadMaximaBasura)
        {
            GenerarBasura();
            tiempoSiguienteSpawn = Time.time + tiempoEntreSpawns;
        }
    }

    void GenerarBasura()
    {
        // Elegir basura basado en probabilidad
        GameObject prefabElegido = ElegirPrefabPorProbabilidad();

        if (prefabElegido == null)
        {
            Debug.LogWarning("No se pudo elegir prefab de basura.");
            return;
        }

        Vector3 posicion = new Vector3(
            Random.Range(-rangoMaximoX, rangoMaximoX),
            alturaBasura,
            Random.Range(-rangoMaximoZ, rangoMaximoZ)
        );

        Quaternion rotacion = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        Instantiate(prefabElegido, posicion, rotacion);

        basuraActual++;
        ActualizarUI();
    }

    GameObject ElegirPrefabPorProbabilidad()
    {
        float total = 0f;
        foreach (var tipo in tiposBasura)
        {
            total += tipo.probabilidad;
        }

        float valorAleatorio = Random.Range(0f, total);
        float suma = 0f;

        foreach (var tipo in tiposBasura)
        {
            suma += tipo.probabilidad;
            if (valorAleatorio <= suma)
            {
                return tipo.prefab;
            }
        }

        return null; // Esto no debería pasar si las probabilidades están bien configuradas
    }

    public void RecogerBasura(string tipo)
    {
        basuraActual = Mathf.Max(0, basuraActual - 1);

        switch (tipo)
        {
            case "Papel":
                contadorPapel++;
                break;
            case "Plastico":
                contadorPlastico++;
                break;
            case "Organico":
                contadorOrganico++;
                break;
            case "Vidrio":
                contadorVidrio++;
                break;
            default:
                Debug.LogWarning("Tipo de basura no reconocido: " + tipo);
                break;
        }

        ActualizarUI();
    }

    void ActualizarUI()
    {
        if (sliderSuciedad != null)
        {
            sliderSuciedad.value = (float)basuraActual / cantidadMaximaBasura;
        }

        if (textoContadoresBasura != null)
        {
            textoContadoresBasura.text =
                "Papel: " + contadorPapel + "\n" +
                "Plástico: " + contadorPlastico + "\n" +
                "Orgánico: " + contadorOrganico + "\n" +
                "Vidrio: " + contadorVidrio;
        }
    }
}
