using UnityEngine;
using System.Collections;
using System;

public class RotadorConPivote : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Objeto que se rotará alrededor del pivote (actúa como puerta)")]
    public Transform objetoARotar;
    
    [Header("Activadores")]
    [Tooltip("Activador Metal - activará la rotación al hacer click")]
    public Transform activadorMetal;
    
    [Tooltip("Activador Glass - activará la rotación al hacer click")]
    public Transform activadorGlass;
    
    [Header("Objeto que se Desplazará")]
    [Tooltip("Objeto que se desplazará en Z cuando se active la rotación")]
    public Transform objetoADesplazar;
    
    [Tooltip("Distancia que se desplazará el objeto en -Z (1 unidad)")]
    public float distanciaDesplazamientoObjetoZ = 1f;
    
    [Tooltip("Velocidad de desplazamiento del objeto (unidades por segundo). Si es 0, será instantáneo")]
    public float velocidadDesplazamientoObjeto = 3f;
    
    [Header("MetalMid Drop Zone")]
    [Tooltip("Drop zone de MetalMid que se moverá junto con el objeto a desplazar")]
    public MetalMidDropZone metalMidDropZone;
    
    [Header("Otro Objeto a Rotar")]
    [Tooltip("Otro objeto que rotará hacia la derecha cuando el objeto principal vuelva a su posición inicial")]
    public Transform otroObjetoARotar;
    
    [Tooltip("Velocidad de rotación del otro objeto (grados por segundo hacia la derecha)")]
    public float velocidadRotacionOtroObjeto = 90f;
    
    [Tooltip("Eje de rotación del otro objeto")]
    public Vector3 ejeRotacionOtroObjeto = Vector3.right;
    
    [Header("Audio del Otro Objeto")]
    [Tooltip("Clip de audio que se reproduce en loop mientras el otro objeto rota")]
    public AudioClip audioRotacionLoop;
    
    [Tooltip("Clip de audio que se reproduce cuando el otro objeto termina de rotar")]
    public AudioClip audioFinRotacion;
    
    private AudioSource audioSource;
    
    [Header("Configuración del Objeto Activador")]
    [Tooltip("Distancia que se desplazará el objeto activador en Z al ser presionado")]
    public float distanciaDesplazamientoZ = 0.04f;
    
    [Tooltip("Velocidad de desplazamiento del objeto activador (unidades por segundo). Si es 0, será instantáneo")]
    public float velocidadDesplazamiento = 5f;
    
    [Header("Configuración de Rotación")]
    [Tooltip("Ángulo total de rotación en grados")]
    public float anguloRotacion = 90f;
    
    [Tooltip("Eje de rotación")]
    public Vector3 ejeRotacion = Vector3.up;
    
    [Tooltip("Velocidad de rotación (grados por segundo). Si es 0, la rotación será instantánea")]
    public float velocidadRotacion = 90f;
    
    [Header("Activación")]
    [Tooltip("Si está activado, rotará automáticamente al iniciar el juego (al presionar Play)")]
    public bool rotarAlInicio = false;
    
    [Tooltip("Si está activado, se puede activar con clic del mouse")]
    public bool activarConClick = true;
    
    [Tooltip("Si está activado, se puede activar múltiples veces (toggle)")]
    public bool permitirToggle = false;
    
    [Header("Estado")]
    [Tooltip("Ángulo actual de rotación acumulada")]
    [SerializeField] private float anguloActual = 0f;
    
    // Evento que se dispara cuando el objeto vuelve completamente a su posición inicial
    public event Action OnObjetoVuelveAPosicionInicial;
    
    private bool rotando = false;
    private bool rotadoCompletamente = false;
    private Quaternion rotacionInicial;
    private Vector3 posicionInicialObjetoARotar;
    private Vector3 posicionPivote;
    private Vector3 posicionPivoteInicial;
    private Renderer rendererObjeto;
    private Vector3 posicionInicialActivadorMetal;
    private Vector3 posicionInicialActivadorGlass;
    private Vector3 posicionInicialObjetoADesplazar;
    private bool activadorMoviendose = false;
    private bool objetoDesplazandose = false;
    
    void Start()
    {
        // Guardar rotación y posición inicial del objeto
        if (objetoARotar != null)
        {
            rotacionInicial = objetoARotar.rotation;
            posicionInicialObjetoARotar = objetoARotar.position;
            
            // Obtener el renderer del objeto para calcular los bounds
            rendererObjeto = objetoARotar.GetComponent<Renderer>();
            if (rendererObjeto == null)
            {
                // Si no tiene renderer en el objeto principal, buscar en los hijos
                rendererObjeto = objetoARotar.GetComponentInChildren<Renderer>();
            }
            
            // Agregar componente para detectar clicks en los activadores
            if (activarConClick)
            {
                // Configurar Activador Metal
                if (activadorMetal != null)
                {
                    ConfigurarActivador(activadorMetal, "Metal");
                }
                
                // Configurar Activador Glass
                if (activadorGlass != null)
                {
                    ConfigurarActivador(activadorGlass, "Glass");
                }
            }
        }
        
        // Guardar posiciones iniciales de los activadores
        if (activadorMetal != null)
        {
            posicionInicialActivadorMetal = activadorMetal.position;
        }
        
        if (activadorGlass != null)
        {
            posicionInicialActivadorGlass = activadorGlass.position;
        }
        
        // Guardar posición inicial del objeto a desplazar
        if (objetoADesplazar != null)
        {
            posicionInicialObjetoADesplazar = objetoADesplazar.position;
        }
        
        // Calcular posición del pivote automáticamente
        CalcularPosicionPivote();
        posicionPivoteInicial = posicionPivote; // Guardar el pivote inicial
        
        // Inicializar AudioSource si no está asignado
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Rotar automáticamente al inicio si está configurado
        if (rotarAlInicio)
        {
            RotarObjeto(anguloRotacion);
        }
    }
    
    /// <summary>
    /// Configura un activador para detectar clicks
    /// </summary>
    void ConfigurarActivador(Transform activador, string tipo)
    {
        // Verificar que el objeto tenga un Collider (necesario para OnMouseDown)
        Collider col = activador.GetComponent<Collider>();
        if (col == null)
        {
            col = activador.GetComponentInChildren<Collider>();
        }
        
        if (col == null)
        {
            Debug.LogWarning($"⚠️ El activador '{activador.name}' necesita un Collider para detectar clicks. Agregando BoxCollider automáticamente.");
            activador.gameObject.AddComponent<BoxCollider>();
        }
        
        ClickDetector detector = activador.GetComponent<ClickDetector>();
        if (detector == null)
        {
            detector = activador.gameObject.AddComponent<ClickDetector>();
        }
        detector.rotador = this;
        detector.tipoActivador = tipo;
    }
    
    /// <summary>
    /// Calcula automáticamente el pivote en el lado izquierdo del objeto, centrado verticalmente
    /// </summary>
    void CalcularPosicionPivote()
    {
        if (objetoARotar == null)
        {
            Debug.LogWarning("⚠️ No se ha asignado un objeto para rotar.");
            posicionPivote = transform.position;
            return;
        }
        
        // Obtener los bounds del objeto
        Bounds bounds;
        
        if (rendererObjeto != null)
        {
            bounds = rendererObjeto.bounds;
        }
        else
        {
            // Fallback: usar Collider si existe
            Collider collider = objetoARotar.GetComponent<Collider>();
            if (collider != null)
            {
                bounds = collider.bounds;
            }
            else
            {
                // Último fallback: usar la posición del objeto
                Debug.LogWarning("⚠️ No se encontró Renderer ni Collider. Usando posición del objeto como pivote.");
                posicionPivote = objetoARotar.position;
                return;
            }
        }
        
        // Calcular el pivote en el lado izquierdo, centrado verticalmente
        // El pivote está en el mínimo X (lado izquierdo), centrado en Y y Z
        posicionPivote = new Vector3(
            bounds.min.x,           // Lado izquierdo
            bounds.center.y,        // Centrado verticalmente
            bounds.center.z         // Centrado en profundidad
        );
    }
    
    /// <summary>
    /// Método llamado cuando se hace click en un activador
    /// </summary>
    public void OnClickActivador(string tipoActivador)
    {
        if (activarConClick && !rotando && !activadorMoviendose)
        {
            // Mover el activador correspondiente (exactamente igual para todos)
            if (tipoActivador == "Metal" && activadorMetal != null)
            {
                StartCoroutine(MoverActivador(activadorMetal, posicionInicialActivadorMetal));
            }
            else if (tipoActivador == "Glass" && activadorGlass != null)
            {
                StartCoroutine(MoverActivador(activadorGlass, posicionInicialActivadorGlass));
            }
            
            // Rotar el objeto (el desplazamiento se activará cuando termine la rotación)
            if (permitirToggle && rotadoCompletamente)
            {
                RotarObjeto(-anguloRotacion);
            }
            else if (!rotadoCompletamente)
            {
                RotarObjeto(anguloRotacion);
            }
        }
    }
    
    /// <summary>
    /// Mueve el activador hacia adelante en Z y luego lo regresa
    /// </summary>
    private IEnumerator MoverActivador(Transform activador, Vector3 posicionInicial)
    {
        activadorMoviendose = true;
        
        Vector3 posicionPresionada = posicionInicial + Vector3.forward * distanciaDesplazamientoZ;
        
        if (velocidadDesplazamiento > 0f)
        {
            // Mover hacia adelante
            while (Vector3.Distance(activador.position, posicionPresionada) > 0.01f)
            {
                activador.position = Vector3.MoveTowards(
                    activador.position,
                    posicionPresionada,
                    velocidadDesplazamiento * Time.deltaTime
                );
                yield return null;
            }
            
            // Asegurar posición exacta
            activador.position = posicionPresionada;
            
            // Pequeña pausa en la posición presionada
            yield return new WaitForSeconds(0.1f);
            
            // Volver a la posición inicial
            while (Vector3.Distance(activador.position, posicionInicial) > 0.01f)
            {
                activador.position = Vector3.MoveTowards(
                    activador.position,
                    posicionInicial,
                    velocidadDesplazamiento * Time.deltaTime
                );
                yield return null;
            }
            
            // Asegurar posición exacta
            activador.position = posicionInicial;
        }
        else
        {
            // Movimiento instantáneo
            activador.position = posicionPresionada;
            yield return new WaitForSeconds(0.1f);
            activador.position = posicionInicial;
        }
        
        activadorMoviendose = false;
    }
    
    /// <summary>
    /// Mueve el objeto a desplazar en -Z y lo deja en esa posición
    /// </summary>
    private IEnumerator MoverObjetoADesplazar()
    {
        if (objetoADesplazar == null) yield break;
        
        objetoDesplazandose = true;
        
        Vector3 posicionDesplazada = posicionInicialObjetoADesplazar + Vector3.back * distanciaDesplazamientoObjetoZ;
        
        if (velocidadDesplazamientoObjeto > 0f)
        {
            // Mover hacia atrás en -Z
            while (Vector3.Distance(objetoADesplazar.position, posicionDesplazada) > 0.01f)
            {
                objetoADesplazar.position = Vector3.MoveTowards(
                    objetoADesplazar.position,
                    posicionDesplazada,
                    velocidadDesplazamientoObjeto * Time.deltaTime
                );
                
                yield return null;
            }
            
            // Asegurar posición exacta
            objetoADesplazar.position = posicionDesplazada;
        }
        else
        {
            // Movimiento instantáneo
            objetoADesplazar.position = posicionDesplazada;
        }
        
        objetoDesplazandose = false;
    }
    
    /// <summary>
    /// Rota el objeto alrededor del pivote el ángulo especificado
    /// </summary>
    /// <param name="angulo">Ángulo en grados a rotar (positivo o negativo)</param>
    public void RotarObjeto(float angulo)
    {
        if (objetoARotar == null)
        {
            Debug.LogWarning("⚠️ No se ha asignado un objeto para rotar.");
            return;
        }
        
        if (rotando)
        {
            Debug.LogWarning("⚠️ El objeto ya está rotando.");
            return;
        }
        
        // Recalcular el pivote antes de rotar (por si el objeto se movió)
        CalcularPosicionPivote();
        
        if (velocidadRotacion > 0f)
        {
            StartCoroutine(RotarSuavemente(angulo));
        }
        else
        {
            RotarInstantaneamente(angulo);
        }
    }
    
    /// <summary>
    /// Rota el objeto instantáneamente
    /// </summary>
    private void RotarInstantaneamente(float angulo)
    {
        objetoARotar.RotateAround(posicionPivote, ejeRotacion, angulo);
        anguloActual += angulo;
        
        if (Mathf.Abs(anguloActual) >= Mathf.Abs(anguloRotacion))
        {
            rotadoCompletamente = true;
        }
        
        // Desplazar el objeto después de que termine la rotación
        if (objetoADesplazar != null)
        {
            StartCoroutine(MoverObjetoADesplazar());
        }
    }
    
    /// <summary>
    /// Rota el objeto suavemente usando una corrutina
    /// </summary>
    private IEnumerator RotarSuavemente(float anguloTotal)
    {
        rotando = true;
        
        float anguloRestante = anguloTotal;
        float anguloRotado = 0f;
        
        while (Mathf.Abs(anguloRestante) > 0.1f)
        {
            float incremento = velocidadRotacion * Time.deltaTime;
            
            // Asegurarse de no rotar más de lo necesario
            if (Mathf.Abs(incremento) > Mathf.Abs(anguloRestante))
            {
                incremento = anguloRestante;
            }
            
            // Rotar alrededor del pivote
            objetoARotar.RotateAround(posicionPivote, ejeRotacion, incremento);
            
            anguloRotado += incremento;
            anguloRestante -= incremento;
            
            yield return null;
        }
        
        // Ajuste final para asegurar precisión
        if (Mathf.Abs(anguloRestante) > 0.01f)
        {
            objetoARotar.RotateAround(posicionPivote, ejeRotacion, anguloRestante);
            anguloRotado += anguloRestante;
        }
        
        anguloActual += anguloRotado;
        
        if (Mathf.Abs(anguloActual) >= Mathf.Abs(anguloRotacion))
        {
            rotadoCompletamente = true;
        }
        
        rotando = false;
        
        // Desplazar el objeto después de que termine la rotación
        if (objetoADesplazar != null)
        {
            StartCoroutine(MoverObjetoADesplazar());
        }
    }
    
    /// <summary>
    /// Resetea el objeto a su rotación inicial (instantáneo)
    /// </summary>
    public void ResetearRotacion()
    {
        if (objetoARotar == null) return;
        
        StopAllCoroutines();
        objetoARotar.rotation = rotacionInicial;
        anguloActual = 0f;
        rotadoCompletamente = false;
        rotando = false;
    }
    
    /// <summary>
    /// Rota el objeto suavemente hacia su posición inicial usando la misma velocidad de rotación
    /// </summary>
    private IEnumerator RotarHaciaPosicionInicial()
    {
        if (objetoARotar == null) yield break;
        
        rotando = true;
        
        // Calcular el ángulo que necesita rotar para volver a la posición inicial
        float anguloARotar = -anguloActual; // Rotar en dirección contraria
        
        if (Mathf.Abs(anguloARotar) < 0.1f)
        {
            // Ya está en la posición inicial o muy cerca, restaurar posición y rotación exactas
            objetoARotar.position = posicionInicialObjetoARotar;
            objetoARotar.rotation = rotacionInicial;
            anguloActual = 0f;
            rotadoCompletamente = false;
            rotando = false;
            yield break;
        }
        
        // Usar el pivote inicial para asegurar que el objeto vuelva exactamente a su posición
        Vector3 pivoteARotar = posicionPivoteInicial;
        
        float anguloRestante = anguloARotar;
        float anguloRotado = 0f;
        
        if (velocidadRotacion > 0f)
        {
            // Rotar suavemente hacia la posición inicial usando el pivote inicial
            while (Mathf.Abs(anguloRestante) > 0.1f)
            {
                float incremento = velocidadRotacion * Time.deltaTime;
                
                // Asegurarse de rotar en la dirección correcta (negativa para volver)
                if (anguloRestante < 0)
                {
                    incremento = -incremento;
                }
                
                // Asegurarse de no rotar más de lo necesario
                if (Mathf.Abs(incremento) > Mathf.Abs(anguloRestante))
                {
                    incremento = anguloRestante;
                }
                
                // Rotar alrededor del pivote inicial
                objetoARotar.RotateAround(pivoteARotar, ejeRotacion, incremento);
                
                anguloRotado += incremento;
                anguloRestante -= incremento;
                
                yield return null;
            }
            
            // Ajuste final para asegurar precisión
            if (Mathf.Abs(anguloRestante) > 0.01f)
            {
                objetoARotar.RotateAround(pivoteARotar, ejeRotacion, anguloRestante);
                anguloRotado += anguloRestante;
            }
        }
        else
        {
            // Rotación instantánea usando el pivote inicial
            objetoARotar.RotateAround(pivoteARotar, ejeRotacion, anguloARotar);
            anguloRotado = anguloARotar;
        }
        
        // Asegurar que el objeto esté exactamente en la posición y rotación iniciales
        objetoARotar.position = posicionInicialObjetoARotar;
        objetoARotar.rotation = rotacionInicial;
        anguloActual = 0f;
        rotadoCompletamente = false;
        rotando = false;
        
        // Disparar evento cuando el objeto vuelve a su posición inicial
        OnObjetoVuelveAPosicionInicial?.Invoke();
        
        // Rotar el otro objeto hacia la derecha por 10 segundos solo si hay hijos (MetalMid o objeto nuevo)
        if (otroObjetoARotar != null && TieneHijos())
        {
            StartCoroutine(RotarOtroObjetoPorTiempo(10f));
        }
    }
    
    /// <summary>
    /// Verifica si hay hijos en el objeto a desplazar (MetalMid o objeto nuevo)
    /// </summary>
    private bool TieneHijos()
    {
        // Verificar si hay MetalMid en la zona
        if (metalMidDropZone != null)
        {
            var objetosEnZona = metalMidDropZone.GetObjetosEnZona();
            if (objetosEnZona != null && objetosEnZona.Count > 0)
            {
                return true;
            }
            
            // Verificar si hay un objeto instanciado actual (objeto nuevo)
            if (metalMidDropZone.TieneObjetoInstanciado())
            {
                return true;
            }
        }
        
        // También verificar directamente si el objeto a desplazar tiene hijos
        if (objetoADesplazar != null && objetoADesplazar.childCount > 0)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Rota el otro objeto hacia la derecha por un tiempo determinado
    /// </summary>
    /// <param name="tiempoSegundos">Tiempo en segundos que rotará el objeto</param>
    private IEnumerator RotarOtroObjetoPorTiempo(float tiempoSegundos)
    {
        if (otroObjetoARotar == null || velocidadRotacionOtroObjeto <= 0f)
        {
            yield break;
        }
        
        // Reproducir audio en loop si está asignado
        if (audioSource != null && audioRotacionLoop != null)
        {
            audioSource.clip = audioRotacionLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        float tiempoTranscurrido = 0f;
        
        while (tiempoTranscurrido < tiempoSegundos)
        {
            // Rotar hacia la derecha (rotación positiva)
            float rotacionFrame = velocidadRotacionOtroObjeto * Time.deltaTime;
            otroObjetoARotar.Rotate(ejeRotacionOtroObjeto, rotacionFrame, Space.World);
            
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }
        
        // Detener el audio de loop
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
        
        // Reproducir audio de fin de rotación si está asignado
        if (audioSource != null && audioFinRotacion != null)
        {
            audioSource.PlayOneShot(audioFinRotacion);
        }
        
        // Repetir las acciones del objeto a rotar y del objeto a desplazar
        RepetirAccionesObjetos();
    }
    
    /// <summary>
    /// Repite las acciones del objeto a rotar y del objeto a desplazar
    /// </summary>
    private void RepetirAccionesObjetos()
    {
        // Solo repetir las acciones si hay hijos (MetalMid o objeto nuevo)
        if (!TieneHijos())
        {
            return;
        }
        
        // Rotar el objeto nuevamente
        if (objetoARotar != null && !rotando)
        {
            RotarObjeto(anguloRotacion);
        }
        
        // El desplazamiento del objeto se activará automáticamente cuando termine la rotación
        // (ya está implementado en RotarSuavemente y RotarInstantaneamente)
    }
    
    /// <summary>
    /// Obtiene la posición actual del pivote
    /// </summary>
    public Vector3 GetPosicionPivote()
    {
        CalcularPosicionPivote();
        return posicionPivote;
    }
    
    /// <summary>
    /// Verifica si el objeto a desplazar ha terminado de desplazarse
    /// </summary>
    public bool ObjetoDesplazamientoCompletado()
    {
        return !objetoDesplazandose;
    }
    
    /// <summary>
    /// Activa el desplazamiento del objeto a desplazar (llamado desde MetalMidDropZone cuando alcanza capacidad máxima)
    /// Mueve el objeto y los MetalMids a su posición original
    /// </summary>
    public void ActivarDesplazamientoObjeto()
    {
        if (objetoADesplazar != null && !objetoDesplazandose)
        {
            StartCoroutine(MoverObjetoAPosicionOriginal());
        }
    }
    
    /// <summary>
    /// Mueve el objeto a desplazar y los MetalMids a su posición original, y resetea la rotación del objeto
    /// </summary>
    private IEnumerator MoverObjetoAPosicionOriginal()
    {
        if (objetoADesplazar == null) yield break;
        
        objetoDesplazandose = true;
        
        Vector3 posicionActual = objetoADesplazar.position;
        Vector3 posicionObjetivo = posicionInicialObjetoADesplazar;
        
        if (velocidadDesplazamientoObjeto > 0f)
        {
            // Mover hacia la posición original
            // Los MetalMid se moverán automáticamente porque son hijos del objeto a desplazar
            while (Vector3.Distance(objetoADesplazar.position, posicionObjetivo) > 0.01f)
            {
                objetoADesplazar.position = Vector3.MoveTowards(
                    objetoADesplazar.position,
                    posicionObjetivo,
                    velocidadDesplazamientoObjeto * Time.deltaTime
                );
                
                yield return null;
            }
            
            // Asegurar posición exacta
            objetoADesplazar.position = posicionObjetivo;
        }
        else
        {
            // Movimiento instantáneo
            objetoADesplazar.position = posicionObjetivo;
        }
        
        // Rotar el objeto suavemente hacia su posición inicial con la misma velocidad
        yield return StartCoroutine(RotarHaciaPosicionInicial());
        
        // Destruir todos los MetalMid cuando el objeto llegue a su posición inicial
        if (metalMidDropZone != null)
        {
            metalMidDropZone.DestruirTodosLosMetalMid();
        }
        
        objetoDesplazandose = false;
        
        // El evento OnObjetoVuelveAPosicionInicial se dispara dentro de RotarHaciaPosicionInicial
    }
    
    /// <summary>
    /// Vuelve el objeto a desplazar y el objeto a rotar a su posición inicial (sin destruir MetalMid)
    /// </summary>
    public void VolverAPosicionInicial()
    {
        if (objetoADesplazar != null && !objetoDesplazandose)
        {
            StartCoroutine(MoverObjetoAPosicionInicialSinDestruir());
        }
    }
    
    /// <summary>
    /// Mueve el objeto a desplazar a su posición original y resetea la rotación del objeto (sin destruir MetalMid)
    /// </summary>
    private IEnumerator MoverObjetoAPosicionInicialSinDestruir()
    {
        if (objetoADesplazar == null) yield break;
        
        objetoDesplazandose = true;
        
        Vector3 posicionObjetivo = posicionInicialObjetoADesplazar;
        
        if (velocidadDesplazamientoObjeto > 0f)
        {
            // Mover hacia la posición original
            while (Vector3.Distance(objetoADesplazar.position, posicionObjetivo) > 0.01f)
            {
                objetoADesplazar.position = Vector3.MoveTowards(
                    objetoADesplazar.position,
                    posicionObjetivo,
                    velocidadDesplazamientoObjeto * Time.deltaTime
                );
                
                yield return null;
            }
            
            // Asegurar posición exacta
            objetoADesplazar.position = posicionObjetivo;
        }
        else
        {
            // Movimiento instantáneo
            objetoADesplazar.position = posicionObjetivo;
        }
        
        // Rotar el objeto suavemente hacia su posición inicial con la misma velocidad
        yield return StartCoroutine(RotarHaciaPosicionInicial());
        
        objetoDesplazandose = false;
    }
    
    /// <summary>
    /// Dibuja un gizmo en el editor para visualizar el pivote
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (objetoARotar == null) return;
        
        // Calcular el pivote temporalmente para visualización en el editor
        Vector3 pivote = Vector3.zero;
        
        Renderer tempRenderer = objetoARotar.GetComponent<Renderer>();
        if (tempRenderer == null)
        {
            tempRenderer = objetoARotar.GetComponentInChildren<Renderer>();
        }
        
        if (tempRenderer != null)
        {
            Bounds bounds = tempRenderer.bounds;
            pivote = new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
        }
        else
        {
            Collider tempCollider = objetoARotar.GetComponent<Collider>();
            if (tempCollider != null)
            {
                Bounds bounds = tempCollider.bounds;
                pivote = new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
            }
            else
            {
                pivote = objetoARotar.position;
            }
        }
        
        // Dibujar esfera en el pivote (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pivote, 0.2f);
        
        // Dibujar línea desde el pivote al centro del objeto (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pivote, objetoARotar.position);
        
        // Dibujar un pequeño cubo en el lado izquierdo para referencia
        Gizmos.color = Color.cyan;
        if (tempRenderer != null)
        {
            Bounds bounds = tempRenderer.bounds;
            Vector3 esquinaIzquierda = new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
            Gizmos.DrawWireCube(esquinaIzquierda, Vector3.one * 0.1f);
        }
    }
}

/// <summary>
/// Componente auxiliar para detectar clicks en los activadores
/// </summary>
public class ClickDetector : MonoBehaviour
{
    public RotadorConPivote rotador;
    public string tipoActivador; // "Metal" o "Glass"
    
    void OnMouseDown()
    {
        if (rotador != null && !string.IsNullOrEmpty(tipoActivador))
        {
            rotador.OnClickActivador(tipoActivador);
        }
    }
}

