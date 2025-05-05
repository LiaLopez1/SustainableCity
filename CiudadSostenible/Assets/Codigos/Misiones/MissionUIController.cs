using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionUIController : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelMision;
    public GameObject imageExclamacion;
    public TextMeshProUGUI textoTituloMision;
    public TextMeshProUGUI textoExplicacion;
    public Button botonCerrar;

    private MissionManager missionManager;

    void Start()
    {
        // Buscar el MissionManager en la escena
        missionManager = FindObjectOfType<MissionManager>();

        // Asegurarse de que el panel está oculto al inicio
        panelMision.SetActive(false);
        botonCerrar.onClick.AddListener(CerrarPanel);
    }

    public void MostrarPanelMision()
    {
        if (missionManager == null) return;

        var mision = missionManager.ObtenerMisionActual();
        if (mision == null) return;

        textoTituloMision.text = mision.nombreMision;
        textoExplicacion.text = mision.descripcion;

        panelMision.SetActive(true);
        imageExclamacion.SetActive(false); // Ocultamos el ícono de exclamación

        mision.fueMostradaAlJugador = true;
    }

    public void CerrarPanel()
    {
        panelMision.SetActive(false);
    }

    public void ActualizarExclamacion()
    {
        var mision = missionManager.ObtenerMisionActual();
        if (mision != null && !mision.fueMostradaAlJugador)
        {
            imageExclamacion.SetActive(true);
        }
        else
        {
            imageExclamacion.SetActive(false);
        }
    }
}
