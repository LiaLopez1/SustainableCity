using UnityEngine;

public class Basura : MonoBehaviour
{
    [Header("Configuraci�n")]
    public string tipoBasura; // Ejemplo: "Papel", "Plastico", "Organico", "Vidrio"

    private bool jugadorCerca = false;
    private UIRecolectarBasura uiRecolectar; // Referencia al controlador de UI

    void Start()
    {
        uiRecolectar = FindObjectOfType<UIRecolectarBasura>();
        if (uiRecolectar == null)
        {
            Debug.LogError("No se encontr� el UIRecolectarBasura en la escena.");
        }
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            Recoger();
        }
    }

    private void Recoger()
    {
        BasuraSpawner spawner = FindObjectOfType<BasuraSpawner>();
        if (spawner != null)
        {
            spawner.RecogerBasura(tipoBasura);
        }
        else
        {
            Debug.LogWarning("No se encontr� el BasuraSpawner en la escena.");
        }

        uiRecolectar?.OcultarMensaje(); // Oculta el mensaje
        Destroy(gameObject); // Destruye la basura recogida
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            uiRecolectar?.MostrarMensaje("Oprime E para recoger");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            uiRecolectar?.OcultarMensaje();
        }
    }
}
