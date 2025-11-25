using UnityEngine;
using System.Collections;

public class PalancaClick : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Ángulo máximo de rotación hacia abajo")]
    public float anguloRotacion = 45f;
    
    [Tooltip("Velocidad de rotación hacia abajo")]
    public float velocidadBajada = 200f;
    
    [Tooltip("Velocidad de retorno a posición inicial")]
    public float velocidadRetorno = 150f;
    
    [Tooltip("Eje de rotación (normalmente X para rotar hacia adelante/atrás)")]
    public Vector3 ejeRotacion = Vector3.right;
    
    [Header("Opcional - Acción al activar")]
    public PlasticBowlCounter bowlCounter;
    
    [Header("Objeto conectado")]
    [Tooltip("Objeto que se moverá cuando se active la palanca")]
    public Transform objetoConectado;
    
    [Tooltip("Distancia hacia abajo que se moverá el objeto (en unidades)")]
    public float distanciaMovimiento = 2f;
    
    [Tooltip("Velocidad de movimiento del objeto hacia abajo")]
    public float velocidadMovimientoBajada = 3f;
    
    [Tooltip("Velocidad de movimiento del objeto hacia arriba")]
    public float velocidadMovimientoSubida = 3f;
    
    [Header("Segundo objeto conectado")]
    [Tooltip("Segundo objeto que se moverá cuando se active la palanca")]
    public Transform objetoConectado2;
    
    [Tooltip("Distancia hacia abajo que se moverá el segundo objeto (en unidades)")]
    public float distanciaMovimiento2 = 2f;
    
    [Tooltip("Velocidad de movimiento del segundo objeto hacia abajo")]
    public float velocidadMovimientoBajada2 = 3f;
    
    [Tooltip("Velocidad de movimiento del segundo objeto hacia arriba")]
    public float velocidadMovimientoSubida2 = 3f;
    
    private bool activando = false;
    private Vector3 posicionPivote;
    private Quaternion rotacionInicial;
    private Renderer rendererObjeto;
    private Vector3 posicionInicialObjeto;
    private Vector3 posicionInicialObjeto2;
    
    void Start()
    {
        // Guardar rotación inicial
        rotacionInicial = transform.rotation;
        
        // Calcular posición del pivote en la base del cilindro
        CalcularPivote();
        
        // Guardar posición inicial del objeto conectado
        if (objetoConectado != null)
        {
            posicionInicialObjeto = objetoConectado.position;
        }
        
        // Guardar posición inicial del segundo objeto conectado
        if (objetoConectado2 != null)
        {
            posicionInicialObjeto2 = objetoConectado2.position;
        }
        
        // Buscar bowlCounter automáticamente si no está asignado
        if (bowlCounter == null)
        {
            bowlCounter = FindObjectOfType<PlasticBowlCounter>();
        }
    }
    
    void CalcularPivote()
    {
        rendererObjeto = GetComponent<Renderer>();
        if (rendererObjeto != null)
        {
            // Obtener el bounds del objeto
            Bounds bounds = rendererObjeto.bounds;
            
            // El pivote está en el centro del bounds pero en la parte inferior en Y
            // Usamos el mínimo Y del bounds como posición del pivote
            posicionPivote = new Vector3(
                bounds.center.x,
                bounds.min.y,
                bounds.center.z
            );
        }
        else
        {
            // Fallback: usar la posición del objeto como pivote
            posicionPivote = transform.position;
        }
    }
    
    void OnMouseDown()
    {
        // Solo activar si no está ya en proceso
        if (!activando)
        {
            StartCoroutine(ActivarPalanca());
        }
    }
    
    IEnumerator ActivarPalanca()
    {
        activando = true;
        
        // Actualizar posición del pivote (por si el objeto se movió)
        CalcularPivote();
        
        // Calcular posición objetivo del objeto conectado
        Vector3 posicionObjetivoAbajo = posicionInicialObjeto;
        if (objetoConectado != null)
        {
            posicionObjetivoAbajo = posicionInicialObjeto - Vector3.up * distanciaMovimiento;
        }
        
        // Calcular posición objetivo del segundo objeto conectado
        Vector3 posicionObjetivoAbajo2 = posicionInicialObjeto2;
        if (objetoConectado2 != null)
        {
            posicionObjetivoAbajo2 = posicionInicialObjeto2 - Vector3.up * distanciaMovimiento2;
        }
        
        // FASE 1: Rotar palanca hacia abajo Y mover objetos hacia abajo simultáneamente
        float anguloActual = 0f;
        float distanciaRecorridaObjeto = 0f;
        float distanciaRecorridaObjeto2 = 0f;
        
        while (anguloActual < anguloRotacion)
        {
            float incremento = velocidadBajada * Time.deltaTime;
            anguloActual = Mathf.Min(anguloActual + incremento, anguloRotacion);
            
            // Rotar palanca alrededor del pivote
            transform.RotateAround(posicionPivote, ejeRotacion, incremento);
            
            // Mover objeto conectado hacia abajo
            if (objetoConectado != null)
            {
                float movimientoObjeto = velocidadMovimientoBajada * Time.deltaTime;
                distanciaRecorridaObjeto = Mathf.Min(distanciaRecorridaObjeto + movimientoObjeto, distanciaMovimiento);
                objetoConectado.position = Vector3.Lerp(posicionInicialObjeto, posicionObjetivoAbajo, distanciaRecorridaObjeto / distanciaMovimiento);
            }
            
            // Mover segundo objeto conectado hacia abajo
            if (objetoConectado2 != null)
            {
                float movimientoObjeto2 = velocidadMovimientoBajada2 * Time.deltaTime;
                distanciaRecorridaObjeto2 = Mathf.Min(distanciaRecorridaObjeto2 + movimientoObjeto2, distanciaMovimiento2);
                objetoConectado2.position = Vector3.Lerp(posicionInicialObjeto2, posicionObjetivoAbajo2, distanciaRecorridaObjeto2 / distanciaMovimiento2);
            }
            
            yield return null;
        }
        
        // Opcional: activar acción cuando la palanca está completamente abajo
        if (bowlCounter != null)
        {
            bowlCounter.ProcesarUnaBotellaDirecto();
        }
        
        // Pequeña pausa en la posición baja (opcional)
        yield return new WaitForSeconds(0.1f);
        
        // FASE 2: Volver palanca Y objeto a la posición inicial simultáneamente
        float anguloARotar = anguloActual;
        
        while (anguloARotar > 0f)
        {
            float decremento = velocidadRetorno * Time.deltaTime;
            anguloARotar = Mathf.Max(anguloARotar - decremento, 0f);
            
            // Rotar palanca hacia atrás alrededor del pivote
            transform.RotateAround(posicionPivote, ejeRotacion, -decremento);
            
            // Mover objeto conectado hacia arriba
            if (objetoConectado != null)
            {
                float movimientoObjeto = velocidadMovimientoSubida * Time.deltaTime;
                distanciaRecorridaObjeto = Mathf.Max(distanciaRecorridaObjeto - movimientoObjeto, 0f);
                objetoConectado.position = Vector3.Lerp(posicionInicialObjeto, posicionObjetivoAbajo, distanciaRecorridaObjeto / distanciaMovimiento);
            }
            
            // Mover segundo objeto conectado hacia arriba
            if (objetoConectado2 != null)
            {
                float movimientoObjeto2 = velocidadMovimientoSubida2 * Time.deltaTime;
                distanciaRecorridaObjeto2 = Mathf.Max(distanciaRecorridaObjeto2 - movimientoObjeto2, 0f);
                objetoConectado2.position = Vector3.Lerp(posicionInicialObjeto2, posicionObjetivoAbajo2, distanciaRecorridaObjeto2 / distanciaMovimiento2);
            }
            
            yield return null;
        }
        
        // Asegurar que volvimos exactamente a la posición inicial
        transform.rotation = rotacionInicial;
        
        // Asegurar que el objeto volvió a su posición inicial
        if (objetoConectado != null)
        {
            objetoConectado.position = posicionInicialObjeto;
        }
        
        // Asegurar que el segundo objeto volvió a su posición inicial
        if (objetoConectado2 != null)
        {
            objetoConectado2.position = posicionInicialObjeto2;
        }
        
        activando = false;
    }
    
    // Método público para activar la palanca desde código
    public void Activar()
    {
        if (!activando)
        {
            StartCoroutine(ActivarPalanca());
        }
    }
}

