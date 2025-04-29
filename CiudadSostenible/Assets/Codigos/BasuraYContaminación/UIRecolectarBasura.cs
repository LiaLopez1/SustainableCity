using UnityEngine;
using TMPro;

public class UIRecolectarBasura : MonoBehaviour
{
    public GameObject panelMensaje;
    public TMP_Text textoMensaje;

    public void MostrarMensaje(string mensaje)
    {
        if (panelMensaje != null && textoMensaje != null)
        {
            panelMensaje.SetActive(true);
            textoMensaje.text = mensaje;
        }
    }

    public void OcultarMensaje()
    {
        if (panelMensaje != null)
        {
            panelMensaje.SetActive(false);
        }
    }
}
