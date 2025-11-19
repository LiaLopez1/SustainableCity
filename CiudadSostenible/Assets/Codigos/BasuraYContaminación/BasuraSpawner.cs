using System.Collections.Generic;
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
    [Header("Área de Spawn 1")]
    public float alturaBasura = 0.133f;
    public float xMin = -20f;
    public float xMax = 20f;
    public float zMin = -15f;
    public float zMax = 25f;

    [Header("Área de Spawn 2")]
    public bool usarSegundaArea = false;
    public float alturaBasura2 = 0.133f;
    public float xMin2 = -10f;
    public float xMax2 = 10f;
    public float zMin2 = -10f;
    public float zMax2 = 10f;

    [Header("Límites por zona (se llenan desde ProgresoMundo)")]
    public int maxZona1 = 0;
    public int maxZona2 = 0;

    [Header("Probabilidades por zona (orden = tiposBasura)")]
    public List<float> probabilidadesZona1 = new List<float>();
    public List<float> probabilidadesZona2 = new List<float>();

    [Tooltip("Probabilidad de usar el Área 1 (0–1). El resto se usa para el Área 2.")]
    [Range(0f, 1f)]
    public float probabilidadArea1 = 0.5f;

    [Header("Configuración de Spawn")]
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
        if (tiposBasura == null || tiposBasura.Count == 0)
            return;

        // 1) Elegir en qué zona vamos a spawnear
        int zonaElegida = ElegirZona();

        if (zonaElegida == -1)
            return; // No hay zonas activas

        // 2) Elegir qué tipo de basura según la zona
        GameObject prefabElegido = ElegirPrefabPorProbabilidad(zonaElegida);
        if (prefabElegido == null)
        {
            Debug.LogWarning("No se pudo elegir prefab de basura.");
            return;
        }

        // 3) Calcular posición según la zona
        Vector3 posicion;

        if (zonaElegida == 0)
        {
            posicion = new Vector3(
                Random.Range(xMin, xMax),
                alturaBasura,
                Random.Range(zMin, zMax)
            );
        }
        else
        {
            posicion = new Vector3(
                Random.Range(xMin2, xMax2),
                alturaBasura2,
                Random.Range(zMin2, zMax2)
            );
        }

        Quaternion rotacion = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        Instantiate(prefabElegido, posicion, rotacion);
        basuraActual++;
    }


    GameObject ElegirPrefabPorProbabilidad(int zona)
    {
        List<float> listaProbs = null;

        if (zona == 0)
            listaProbs = probabilidadesZona1;
        else if (zona == 1)
            listaProbs = probabilidadesZona2;

        // Si no hay lista para esa zona, usamos las probabilidades del tipo
        bool usarProbGlobal = (listaProbs == null || listaProbs.Count == 0);

        float total = 0f;

        if (usarProbGlobal)
        {
            foreach (var tipo in tiposBasura)
                total += tipo.probabilidad;
        }
        else
        {
            int count = tiposBasura.Count;
            for (int i = 0; i < count; i++)
            {
                float p = (i < listaProbs.Count) ? listaProbs[i] : 0f;
                total += p;
            }
        }

        if (total <= 0f)
            return null;

        float valorAleatorio = Random.Range(0f, total);
        float suma = 0f;

        if (usarProbGlobal)
        {
            foreach (var tipo in tiposBasura)
            {
                suma += tipo.probabilidad;
                if (valorAleatorio <= suma)
                    return tipo.prefab;
            }
        }
        else
        {
            int count = tiposBasura.Count;
            for (int i = 0; i < count; i++)
            {
                float p = (i < listaProbs.Count) ? listaProbs[i] : 0f;
                suma += p;
                if (valorAleatorio <= suma)
                    return tiposBasura[i].prefab;
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

    private void OnDrawGizmos()
    {
        // --------- Área 1 ----------
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);

        Vector3 centro1 = new Vector3(
            (xMin + xMax) / 2f,
            alturaBasura,
            (zMin + zMax) / 2f
        );

        Vector3 tamaño1 = new Vector3(
            Mathf.Abs(xMax - xMin),
            0.1f,
            Mathf.Abs(zMax - zMin)
        );

        Gizmos.DrawCube(centro1, tamaño1);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(centro1, tamaño1);

        // --------- Área 2 (solo si está activa) ----------
        if (usarSegundaArea)
        {
            Gizmos.color = new Color(0f, 0.7f, 1f, 0.25f);

            Vector3 centro2 = new Vector3(
                (xMin2 + xMax2) / 2f,
                alturaBasura2,
                (zMin2 + zMax2) / 2f
            );

            Vector3 tamaño2 = new Vector3(
                Mathf.Abs(xMax2 - xMin2),
                0.1f,
                Mathf.Abs(zMax2 - zMin2)
            );

            Gizmos.DrawCube(centro2, tamaño2);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(centro2, tamaño2);
        }
    }

    // Llamado por ProgresoMundo para copiar las probabilidades
    public void SetProbabilidadesZona(int indiceZona, List<float> nuevasProbs)
    {
        if (tiposBasura == null) return;

        List<float> destino = (indiceZona == 0) ? probabilidadesZona1 : probabilidadesZona2;
        if (destino == null)
            destino = new List<float>();

        destino.Clear();

        int tipos = tiposBasura.Count;
        for (int i = 0; i < tipos; i++)
        {
            float prob = (nuevasProbs != null && i < nuevasProbs.Count) ? nuevasProbs[i] : 0f;
            destino.Add(prob);
        }

        if (indiceZona == 0) probabilidadesZona1 = destino;
        else probabilidadesZona2 = destino;
    }

    int ElegirZona()
    {
        // Caso simple: solo zona 1
        if (!usarSegundaArea || maxZona2 <= 0)
        {
            return (maxZona1 > 0) ? 0 : -1;
        }

        // Si las dos zonas están activas, usamos sus "maxZona" para repartir
        int totalMax = Mathf.Max(0, maxZona1) + Mathf.Max(0, maxZona2);
        if (totalMax <= 0) return -1;

        float valor = Random.Range(0f, totalMax);
        return (valor < maxZona1) ? 0 : 1;
    }


}
