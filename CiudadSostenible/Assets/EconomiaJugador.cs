using UnityEngine;
using TMPro;

public class EconomiaJugador : MonoBehaviour
{
    public static EconomiaJugador Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI textoDinero;

    private int dinero = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ActualizarUI();
    }

    public int ObtenerDinero() => dinero;

    public void EstablecerDinero(int cantidad)
    {
        dinero = cantidad;
        ActualizarUI();
    }

    public bool TieneDinero(int cantidad)
    {
        return dinero >= cantidad;
    }

    public bool RestarDinero(int cantidad)
    {
        if (dinero >= cantidad)
        {
            dinero -= cantidad;
            ActualizarUI();
            return true;
        }
        return false;
    }

    public void AgregarDinero(int cantidad)
    {
        dinero += cantidad;
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoDinero != null)
            textoDinero.text = "$" + dinero;
    }
}
