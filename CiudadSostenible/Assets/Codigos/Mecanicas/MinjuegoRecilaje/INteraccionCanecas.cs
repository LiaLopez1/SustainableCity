using UnityEngine;

public class InteraccionCanecas : MonoBehaviour
{
    public GameObject mensajeInteraccion;  
    public GameObject panelMiniJuego;     
    public GameObject panelInventario;   

    private bool jugadorCerca = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            mensajeInteraccion.SetActive(true); 
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            mensajeInteraccion.SetActive(false); 
            panelMiniJuego.SetActive(false);     
        }
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            bool activo = !panelMiniJuego.activeSelf;
            panelMiniJuego.SetActive(activo);
            panelInventario.SetActive(activo);
            mensajeInteraccion.SetActive(!activo); 
        }
    }
}
