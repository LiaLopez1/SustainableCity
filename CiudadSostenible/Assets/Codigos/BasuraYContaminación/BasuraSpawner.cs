﻿using System.Collections.Generic;
using UnityEngine;

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
    public List<TipoBasura> tiposBasura;

    [Header("Progreso de Recolección")]
    [SerializeField] private int basuraTotalRecogida = 0;

    [Header("Configuración de Spawn")]
    public float alturaBasura = 0.133f;
    public float xMin = -20f;  // Límite izquierdo (X negativo)
    public float xMax = 20f;   // Límite derecho (X positivo)
    public float zMin = -15f;  // Límite inferior (Z negativo)
    public float zMax = 25f;
    public int cantidadMaximaBasura = 50;
    public float tiempoTotalGeneracion = 60f;

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
        GameObject prefabElegido = ElegirPrefabPorProbabilidad();
        if (prefabElegido == null)
        {
            Debug.LogWarning("No se pudo elegir prefab de basura.");
            return;
        }

        Vector3 posicion = new Vector3(
            Random.Range(xMin, xMax),  // Usa los límites personalizados en X
            alturaBasura,
            Random.Range(zMin, zMax)   // Usa los límites personalizados en Z
        );

        Quaternion rotacion = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        Instantiate(prefabElegido, posicion, rotacion);
        basuraActual++;
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

        return null;
    }

    public void RecogerBasura(string tipo)
    {
        basuraActual = Mathf.Max(0, basuraActual - 1);
        basuraTotalRecogida++;

        // Generar una nueva basura si ya se completó la generación inicial
        if (basuraActual < cantidadMaximaBasura && Time.time > tiempoSiguienteSpawn)
        {
            GenerarBasura();
        }
    }


    public int GetBasuraRecogida()
    {
        return basuraTotalRecogida;
    }
}
