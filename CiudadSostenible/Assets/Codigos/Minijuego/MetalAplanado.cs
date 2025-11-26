using UnityEngine;

public class MetalAplanado : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Prefab que reemplazará al objeto Metal cuando sea aplanado")]
    public GameObject prefabAplanado;
    
    [Tooltip("Tag del objeto que aplana (normalmente 'Aplanadora')")]
    public string tagAplanadora = "Aplanadora";
    
    [Header("Efectos opcionales")]
    [Tooltip("Sonido al ser aplanado")]
    public AudioClip sonidoAplanado;
    
    [Tooltip("Partículas al ser aplanado")]
    public GameObject efectoParticulas;
    
    [Header("Configuración BoxCollider del prefab aplanado")]
    [Tooltip("Tamaño del BoxCollider del prefab aplanado")]
    public Vector3 tamanioBoxColliderAplanado = new Vector3(276.2457f, 12.98422f, 292.224f);
    
    [Tooltip("Centro del BoxCollider del prefab aplanado")]
    public Vector3 centroBoxColliderAplanado = new Vector3(-7.17276f, 167.9172f, -260f);
    
    private bool yaAplanado = false;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(tagAplanadora) && !yaAplanado)
        {
            AplanarMetal();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagAplanadora) && !yaAplanado)
        {
            AplanarMetal();
        }
    }
    
    void AplanarMetal()
    {
        if (yaAplanado || prefabAplanado == null) return;
        
        yaAplanado = true;
        
        Vector3 posicionFinal = transform.position;
        
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        gameObject.SetActive(false);
        
        if (sonidoAplanado != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoAplanado);
        }
        
        if (efectoParticulas != null)
        {
            Instantiate(efectoParticulas, posicionFinal, Quaternion.identity);
        }
        
        GameObject objetoAplanado = Instantiate(
            prefabAplanado,
            posicionFinal,
            Quaternion.Euler(0f, 0f, 0f)
        );
        
        Renderer[] nuevosRenderers = objetoAplanado.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in nuevosRenderers)
        {
            renderer.enabled = true;
        }
        
        // Cambiar el tamaño del BoxCollider del prefab aplanado
        CambiarTamanioBoxCollider(objetoAplanado);
        
        objetoAplanado.SetActive(true);
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Cambia el tamaño y centro del BoxCollider del objeto aplanado
    /// </summary>
    private void CambiarTamanioBoxCollider(GameObject objeto)
    {
        if (objeto == null) return;
        
        // Buscar BoxCollider en el objeto o sus hijos
        BoxCollider boxCollider = objeto.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = objeto.GetComponentInChildren<BoxCollider>();
        }
        
        if (boxCollider != null)
        {
            boxCollider.size = tamanioBoxColliderAplanado;
            boxCollider.center = centroBoxColliderAplanado;
        }
        else
        {
            Debug.LogWarning($"⚠️ No se encontró BoxCollider en '{objeto.name}' para cambiar su tamaño y centro.");
        }
    }
    
    public void ForzarAplanado()
    {
        if (!yaAplanado)
        {
            AplanarMetal();
        }
    }
}

