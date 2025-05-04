using UnityEngine;

public class PuntoDeSiembra : MonoBehaviour
{
    [Header("Estados del minijuego")]
    private bool tieneSemilla = false;
    private bool tieneComposta = false;
    private bool tieneAgua = false;
    private bool minijuegoFinalizado = false;

    [Header("Prefabs y objetos")]
    public GameObject monticuloPrefab;
    public GameObject retoñoPrefab;
    public Transform puntoInstancia;
    public GameObject sliderAguaPrefab;

    private GameObject monticuloInstanciado;
    private GameObject retoñoInstanciado;
    private GameObject sliderAguaInstanciado;

    [Header("Colores")]
    public Color colorRegado = Color.green;
    private Renderer monticuloRenderer;
    private Color colorOriginal;

    [Header("Canvas del minijuego")]
    public GameObject canvasPlantado; // Asigna el canvas que contiene el botón

    [Header("Botón físico de Listo")]
    public GameObject botonListo;

    [Header("Cámara de minijuego")]
    public Transform objetivoCamara; // Asigna el hijo vacío
    public Camera camaraPlantado;   // Asigna la cámara externa

    private Camera camaraOriginal;
    private bool jugadorDentro = false;
    public GameObject textoInteraccion; // Texto flotante o UI en mundo
    private bool minijuegoActivo = false;
    public GameObject jugador;

    private void Start()
    {
        if (botonListo != null)
            botonListo.SetActive(true);

        if (sliderAguaPrefab != null)
            sliderAguaPrefab.SetActive(false);

        if (camaraPlantado != null)
            camaraPlantado.gameObject.SetActive(false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);
    }

    // Llamado cuando se arrastra un ítem a este punto
    public void RecibirItem(ItemData item)
    {
        if (minijuegoFinalizado || item == null) return;

        string nombre = item.itemName.ToLower();

        if (nombre.Contains("Semillas") && !tieneSemilla)
        {
            InstanciarMonticulo();
            tieneSemilla = true;
        }
        else if (nombre.Contains("Compost") && tieneSemilla && !tieneComposta)
        {
            AplicarComposta();
            tieneComposta = true;
        }
        else if (nombre.Contains("Bidon Plastico") && tieneSemilla && tieneComposta && !tieneAgua)
        {
            if (BidonDeAguaManager.Instance.EstaLleno(item))
            {
                AplicarAgua();
                tieneAgua = true;
                BidonDeAguaManager.Instance.VaciarBidon(item);
            }
            else
            {
                Debug.Log("Este bidón está vacío.");
            }
        }
    }


    private void InstanciarMonticulo()
    {
        if (monticuloInstanciado != null) return;

        monticuloInstanciado = Instantiate(monticuloPrefab, puntoInstancia.position, Quaternion.identity);
        monticuloRenderer = monticuloInstanciado.GetComponentInChildren<Renderer>();

        if (monticuloRenderer != null)
            colorOriginal = monticuloRenderer.material.color;
    }

    private void AplicarComposta()
    {
        if (monticuloInstanciado == null) return;

        monticuloInstanciado.transform.position += new Vector3(0, 0.2f, 0); // Eleva ligeramente
    }

    private void AplicarAgua()
    {
        if (monticuloRenderer != null)
            monticuloRenderer.material.color = colorRegado;
    }

    private void DesactivarJugador()
    {
        if (jugador != null)
            jugador.SetActive(false);
    }

    private void ActivarJugador()
    {
        if (jugador != null)
            jugador.SetActive(true);
    }

    public void PresionarBotonListo()
    {
        Debug.Log("Se presionó Listo en: " + gameObject.name);

        if (minijuegoFinalizado) return;

        if (tieneSemilla && tieneComposta && tieneAgua)
        {
            InstanciarRetoño();
            minijuegoFinalizado = true;
        }
        canvasPlantado.SetActive(false);
        DesactivarCamaraPlantado();
        ActivarJugador();
    }


    private void InstanciarRetoño()
    {
        retoñoInstanciado = Instantiate(retoñoPrefab, puntoInstancia.position, Quaternion.identity);

        if (sliderAguaPrefab != null)
        {
            sliderAguaInstanciado = Instantiate(sliderAguaPrefab, retoñoInstanciado.transform);
            sliderAguaInstanciado.SetActive(true);
        }

        if (monticuloInstanciado != null)
            Destroy(monticuloInstanciado);
    }

    public void ActivarCamaraPlantado()
    {
        ControladorPlantadoGlobal.Instance?.EstablecerPuntoActivo(this);
        if (camaraPlantado == null || objetivoCamara == null) return;

        camaraOriginal = Camera.main;
        if (camaraOriginal != null)
            camaraOriginal.gameObject.SetActive(false);

        camaraPlantado.transform.SetPositionAndRotation(objetivoCamara.position, objetivoCamara.rotation);
        camaraPlantado.gameObject.SetActive(true);

        if (canvasPlantado != null)
            canvasPlantado.SetActive(true); // ✅ Activa el canvas en el mismo momento
    }

    public void DesactivarCamaraPlantado()
    {
        if (camaraPlantado != null)
            camaraPlantado.gameObject.SetActive(false);

        if (camaraOriginal != null)
            camaraOriginal.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !minijuegoFinalizado)
        {
            jugadorDentro = true;
            textoInteraccion?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            textoInteraccion?.SetActive(false);
        }
    }


    private void Update()
    {
        if (jugadorDentro && Input.GetKeyDown(KeyCode.E) && !minijuegoFinalizado && !minijuegoActivo)
        {
            ActivarCamaraPlantado();
            textoInteraccion?.SetActive(false);
            DesactivarJugador();
            minijuegoActivo = true;
        }
    }


    public bool EstaListo() => minijuegoFinalizado;

    public GameObject ObtenerRetoño() => retoñoInstanciado;
}
