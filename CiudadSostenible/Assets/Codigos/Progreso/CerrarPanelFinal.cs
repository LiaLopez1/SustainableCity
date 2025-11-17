using UnityEngine;

public class CerrarPanelFinal : MonoBehaviour
{
    public GameObject panelFinal;
    public GameObject jugador; // GameObject que tiene el PlayerMovement

    private PlayerMovement movimiento;

    void Start()
    {
        if (jugador != null)
            movimiento = jugador.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Solo bloquea si el panel está activo
        if (panelFinal != null && movimiento != null)
        {
            movimiento.enabled = !panelFinal.activeSelf;
        }
    }

    public void CerrarPanel()
    {
        if (panelFinal != null)
            panelFinal.SetActive(false);
    }
}
