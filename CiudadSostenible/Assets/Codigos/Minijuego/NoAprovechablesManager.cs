using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NoAprovechablesManager : MonoBehaviour
{
    [Header("Puntos fijos de spawn (6 en total)")]
    public List<Transform> puntosDeSpawn = new List<Transform>();

    [Header("Dependencias")]
    public CamionFullCapacity camion;

    private int posicionActual = 0;

    public bool PuedeRecibirObjeto()
    {
        if (camion != null && camion.EstaLleno)
        {
            MostrarMensajeLleno();
            return false;
        }

        return true;
    }

    public Vector3 CalcularPosicionSpawn()
    {
        if (puntosDeSpawn == null || puntosDeSpawn.Count == 0)
        {
            Debug.LogWarning("⚠️ No se han asignado puntos de spawn.");
            return Vector3.zero;
        }

        // Usamos posición circular entre los 6 puntos
        Transform punto = puntosDeSpawn[posicionActual % puntosDeSpawn.Count];
        posicionActual++;
        return punto.position;
    }

    public void ReiniciarPosiciones()
    {
        posicionActual = 0;
    }

    private void MostrarMensajeLleno()
    {
        if (camion != null && camion.mensajeTMP != null)
        {
            camion.mensajeTMP.gameObject.SetActive(true);
            StartCoroutine(OcultarTMPDespuesDe(camion.mensajeTMP, 2f));
        }
    }

    private IEnumerator OcultarTMPDespuesDe(TextMeshProUGUI tmp, float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (tmp != null)
            tmp.gameObject.SetActive(false);
    }
}
