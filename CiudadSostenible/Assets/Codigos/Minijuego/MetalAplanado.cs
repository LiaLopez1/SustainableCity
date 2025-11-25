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
        
        objetoAplanado.SetActive(true);
        
        Destroy(gameObject);
    }
    
    public void ForzarAplanado()
    {
        if (!yaAplanado)
        {
            AplanarMetal();
        }
    }
}

