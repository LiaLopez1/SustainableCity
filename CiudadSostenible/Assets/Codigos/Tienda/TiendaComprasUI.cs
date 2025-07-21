using UnityEngine;
using TMPro;

public class TiendaComprasUI : MonoBehaviour
{
    [Header("Referencias")]
    public InventorySystem inventario;
    public TextMeshProUGUI textoDinero;
    public TextMeshProUGUI textoAdvertencia;

    [Header("Estado actual")]
    [SerializeField] private int dinero = 0;

    public bool TieneDineroSuficiente(int cantidad)
    {
        return EconomiaJugador.Instance.TieneDinero(cantidad);
    }

    public void RestarDinero(int cantidad)
    {
        EconomiaJugador.Instance.RestarDinero(cantidad);
    }

    public bool AnadirAlInventario(ItemData item)
    {
        return inventario.AddItem(item);
    }

    public void MostrarMensaje(string mensaje)
    {
        Debug.Log("MENSAJE: " + mensaje);
        if (textoAdvertencia != null)
        {
            textoAdvertencia.text = mensaje;
            textoAdvertencia.gameObject.SetActive(true);

            CancelInvoke(nameof(OcultarMensaje));
            Invoke(nameof(OcultarMensaje), 2f);
        }
    }


    private void OcultarMensaje()
    {
        if (textoAdvertencia != null)
        {
            textoAdvertencia.gameObject.SetActive(false);
        }
    }

    // Actualización de UI (opcional)
    public void EstablecerDinero(int cantidad)
    {
        dinero = cantidad;
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoDinero != null)
            textoDinero.text = "$" + dinero;
    }

    public int ObtenerDinero() => dinero;


}
