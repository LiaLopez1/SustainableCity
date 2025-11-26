using UnityEngine;
using System.Collections;
using TMPro;

public class MetalMidDropZone : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tag del objeto que se puede soltar aquí (MetalMid)")]
    public string targetTag = "MetalMid";
    
    [Tooltip("Pivote donde caerán los objetos")]
    public Transform pivoteCaida;
    
    [Header("Cámara requerida")]
    [Tooltip("Cámara que debe estar activa para que funcione el drop zone")]
    public Camera camaraRequerida;
    
    [Header("Configuración de Capacidad")]
    [Tooltip("Capacidad máxima de objetos MetalMid")]
    public int capacidadMaxima = 2;
    
    [Tooltip("Distancia en Z para el segundo objeto MetalMid")]
    public float distanciaSegundoMetalMidZ = 0.2f;
    
    [Tooltip("Mensaje que se muestra cuando se alcanza la capacidad máxima")]
    public TMPro.TextMeshProUGUI mensajeFullCapacity;
    
    [Header("Rotador Con Pivote")]
    [Tooltip("RotadorConPivote que desplazará el objeto cuando se alcance la capacidad máxima")]
    public RotadorConPivote rotadorConPivote;
    
    [Tooltip("Si está activado, los MetalMid serán hijos del objeto a desplazar para moverse con él")]
    public bool hacerMetalMidsHijos = true;
    
    [Header("Objeto a Aparecer")]
    [Tooltip("Objeto que aparecerá después de destruir los MetalMid")]
    public GameObject objetoAAparecer;
    
    [Tooltip("Posición donde aparecerá el objeto")]
    public Vector3 posicionAparicion = new Vector3(0.284596384f, 0.793167174f, -11.6230383f);
    
    [Header("Estado")]
    [Tooltip("Objetos actualmente en la zona")]
    private System.Collections.Generic.List<GameObject> objetosEnZona = new System.Collections.Generic.List<GameObject>();
    
    [Tooltip("Referencia al objeto instanciado que apareció")]
    private GameObject objetoInstanciadoActual = null;
    
    private void Start()
    {
        // Si no se asigna el pivote, usar la posición del objeto
        if (pivoteCaida == null)
        {
            pivoteCaida = transform;
        }
        
        // Asegurar que hay un Collider como Trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"⚠️ El objeto '{gameObject.name}' necesita un Collider para detectar objetos. Agregando BoxCollider automáticamente.");
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"⚠️ El Collider del objeto '{gameObject.name}' debe ser un Trigger. Activando isTrigger automáticamente.");
            col.isTrigger = true;
        }
    }
    
    /// <summary>
    /// Verifica si la cámara requerida está activa
    /// </summary>
    private bool IsCameraActive()
    {
        if (camaraRequerida == null)
        {
            Debug.LogWarning("⚠️ No se asignó una cámara requerida.");
            return false;
        }
        
        return camaraRequerida.enabled && camaraRequerida.gameObject.activeInHierarchy;
    }
    
    /// <summary>
    /// Detecta cuando un objeto entra en el trigger
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!IsCameraActive())
        {
            return;
        }
        
        // Verificar si el objeto tiene el tag correcto
        if (other.CompareTag(targetTag))
        {
            ProcesarObjeto(other.gameObject);
        }
    }
    
    /// <summary>
    /// Detecta cuando se arrastra un objeto sobre la zona (usando raycast desde el mouse)
    /// </summary>
    public void OnDropObjeto(GameObject objeto)
    {
        if (!IsCameraActive())
        {
            Debug.Log("⚠️ La cámara requerida no está activa.");
            return;
        }
        
        if (objeto == null) return;
        
        // Verificar el tag del objeto
        if (objeto.CompareTag(targetTag))
        {
            ProcesarObjeto(objeto);
        }
    }
    
    /// <summary>
    /// Procesa el objeto que cae en la zona
    /// </summary>
    private void ProcesarObjeto(GameObject objeto)
    {
        if (objeto == null || pivoteCaida == null) return;
        
        // Verificar si el objeto ya está en la lista
        if (objetosEnZona.Contains(objeto))
        {
            return;
        }
        
        objetosEnZona.Add(objeto);
        
        // Colocar el objeto directamente en el pivote
        ColocarObjetoEnPivote(objeto);
    }
    
    /// <summary>
    /// Verifica si puede recibir más objetos (no ha alcanzado la capacidad máxima)
    /// </summary>
    public bool PuedeRecibirMasObjetos()
    {
        return objetosEnZona.Count < capacidadMaxima;
    }
    
    /// <summary>
    /// Obtiene la cantidad actual de objetos en la zona
    /// </summary>
    public int GetCantidadObjetos()
    {
        return objetosEnZona.Count;
    }
    
    /// <summary>
    /// Obtiene la posición de spawn según la cantidad de objetos actuales
    /// </summary>
    public Vector3 GetPosicionSpawn()
    {
        Vector3 posicionBase = pivoteCaida != null ? pivoteCaida.position : transform.position;
        
        if (objetosEnZona.Count == 0)
        {
            // Primer objeto: posición base
            return posicionBase;
        }
        else if (objetosEnZona.Count == 1)
        {
            // Segundo objeto: posición en +Z
            return posicionBase + Vector3.forward * distanciaSegundoMetalMidZ;
        }
        
        // Si hay más objetos, devolver la posición base (no debería llegar aquí si se verifica la capacidad)
        return posicionBase;
    }
    
    /// <summary>
    /// Muestra el mensaje de capacidad máxima
    /// </summary>
    public void MostrarMensajeFullCapacity()
    {
        if (mensajeFullCapacity != null)
        {
            mensajeFullCapacity.gameObject.SetActive(true);
            StartCoroutine(OcultarMensajeDespuesDe(2f));
        }
    }
    
    /// <summary>
    /// Oculta el mensaje después de un tiempo
    /// </summary>
    private IEnumerator OcultarMensajeDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (mensajeFullCapacity != null)
        {
            mensajeFullCapacity.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Agrega un objeto a la lista de objetos en la zona
    /// </summary>
    public void AgregarObjeto(GameObject objeto)
    {
        if (objeto != null && !objetosEnZona.Contains(objeto))
        {
            objetosEnZona.Add(objeto);
            
            // Hacer que el objeto sea estático (desactivar física)
            HacerObjetoEstatico(objeto);
            
            // Verificar si se alcanzó la capacidad máxima
            if (objetosEnZona.Count >= capacidadMaxima)
            {
                ActivarDesplazamiento();
            }
        }
    }
    
    /// <summary>
    /// Hace que un objeto sea estático para evitar que salga volando por colisiones
    /// </summary>
    private void HacerObjetoEstatico(GameObject objeto)
    {
        if (objeto == null) return;
        
        // Desactivar o hacer kinematic el Rigidbody
        Rigidbody rb = objeto.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Hacer kinematic para que no se vea afectado por física pero pueda moverse manualmente
        }
        
        // También buscar en los hijos por si el Rigidbody está en un objeto hijo
        Rigidbody[] rbs = objeto.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody childRb in rbs)
        {
            if (childRb != rb) // Evitar duplicar el que ya procesamos
            {
                childRb.isKinematic = true;
            }
        }
        
        // Hacer que el objeto sea hijo del objeto a desplazar si está configurado
        if (hacerMetalMidsHijos && rotadorConPivote != null && rotadorConPivote.objetoADesplazar != null)
        {
            objeto.transform.SetParent(rotadorConPivote.objetoADesplazar, true);
        }
    }
    
    /// <summary>
    /// Activa el desplazamiento del objeto cuando se alcanza la capacidad máxima
    /// </summary>
    private void ActivarDesplazamiento()
    {
        if (rotadorConPivote != null)
        {
            // Activar el desplazamiento del objeto a desplazar
            rotadorConPivote.ActivarDesplazamientoObjeto();
        }
    }
    
    /// <summary>
    /// Remueve un objeto de la lista
    /// </summary>
    public void RemoverObjeto(GameObject objeto)
    {
        if (objeto != null)
        {
            objetosEnZona.Remove(objeto);
        }
    }
    
    /// <summary>
    /// Obtiene la lista de objetos en la zona (para que RotadorConPivote pueda moverlos)
    /// </summary>
    public System.Collections.Generic.List<GameObject> GetObjetosEnZona()
    {
        return objetosEnZona;
    }
    
    /// <summary>
    /// Coloca el objeto directamente en la posición del pivote
    /// </summary>
    private void ColocarObjetoEnPivote(GameObject objeto)
    {
        if (objeto == null || pivoteCaida == null) return;
        
        // Colocar el objeto en la posición exacta del pivote
        objeto.transform.position = pivoteCaida.position;
        
        // Opcional: hacer que el objeto sea hijo del pivote
        objeto.transform.SetParent(pivoteCaida, true);
    }
    
    /// <summary>
    /// Método público para que InventoryItemDragHandler pueda llamar cuando se suelta un objeto
    /// </summary>
    public bool PuedeRecibirObjeto(string itemTag)
    {
        if (!IsCameraActive())
        {
            return false;
        }
        
        return itemTag == targetTag;
    }
    
    /// <summary>
    /// Obtiene la posición del pivote para spawn
    /// </summary>
    public Vector3 GetPosicionPivote()
    {
        if (pivoteCaida != null)
        {
            return pivoteCaida.position;
        }
        return transform.position;
    }
    
    /// <summary>
    /// Remueve un objeto de la lista cuando sale de la zona
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            RemoverObjeto(other.gameObject);
        }
    }
    
    /// <summary>
    /// Destruye todos los MetalMid en la zona y hace aparecer un objeto en la posición especificada
    /// </summary>
    public void DestruirTodosLosMetalMid()
    {
        // Crear una copia de la lista para evitar problemas al modificar durante la iteración
        var objetosACopiar = new System.Collections.Generic.List<GameObject>(objetosEnZona);
        
        foreach (GameObject metalMid in objetosACopiar)
        {
            if (metalMid != null)
            {
                Destroy(metalMid);
            }
        }
        
        // Limpiar la lista después de destruir los objetos
        objetosEnZona.Clear();
        
        // Hacer aparecer el objeto en la posición especificada
        if (objetoAAparecer != null)
        {
            objetoInstanciadoActual = Instantiate(objetoAAparecer, posicionAparicion, Quaternion.identity);
            
            // Hacer que el objeto sea hijo del objeto a desplazar si está configurado (igual que los MetalMid)
            if (hacerMetalMidsHijos && rotadorConPivote != null && rotadorConPivote.objetoADesplazar != null)
            {
                objetoInstanciadoActual.transform.SetParent(rotadorConPivote.objetoADesplazar, true);
            }
            
            // Iniciar la verificación para detectar cuando el objeto sea recogido
            StartCoroutine(VerificarObjetoRecogido());
        }
    }
    
    /// <summary>
    /// Verifica periódicamente si el objeto instanciado ya fue recogido (ya no existe)
    /// </summary>
    private IEnumerator VerificarObjetoRecogido()
    {
        // Verificar cada frame si el objeto ya no existe
        while (objetoInstanciadoActual != null)
        {
            yield return null;
        }
        
        // El objeto ya no existe, volver a la posición inicial
        if (rotadorConPivote != null)
        {
            rotadorConPivote.VolverAPosicionInicial();
        }
        
        objetoInstanciadoActual = null;
    }
    
    /// <summary>
    /// Verifica si hay un objeto instanciado actualmente
    /// </summary>
    public bool TieneObjetoInstanciado()
    {
        return objetoInstanciadoActual != null;
    }
    
}

