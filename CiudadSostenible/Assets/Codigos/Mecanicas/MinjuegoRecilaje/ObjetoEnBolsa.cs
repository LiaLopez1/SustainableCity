using UnityEngine;

public class ObjetoEnBolsa : MonoBehaviour
{
    public DetectorBolsa detector;

    void OnDestroy()
    {
        if (detector != null)
            detector.RevisarSiBolsaEstaVacia();
    }
}
