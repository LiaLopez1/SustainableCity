using UnityEngine;

public class ControladorPlantadoGlobal : MonoBehaviour
{
    public static ControladorPlantadoGlobal Instance { get; private set; }

    private PuntoDeSiembraSimple puntoActivo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    public void EstablecerPuntoActivo(PuntoDeSiembraSimple punto)
    {
        puntoActivo = punto;
    }

    /*public void EjecutarBotonListo()
    {
        if (puntoActivo != null)
            puntoActivo.PresionarBotonListo();
        else
            Debug.LogWarning("No hay punto activo de siembra registrado.");
    }*/

}
