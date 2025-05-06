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

    // Se llama desde ProductoComprable
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
        // AddItem ya devuelve false si no hay espacio
        return inventario.AddItem(item);
    }

    public void MostrarMensaje(string mensaje)
    {
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
        textoAdvertencia.gameObject.SetActive(false);
    }


    private void LimpiarMensaje()
    {
        textoAdvertencia.text = "";
    }

    private void ActualizarUI()
    {
        if (textoDinero != null)
            textoDinero.text = "$" + dinero;
    }

    // Puedes usar esto para cargar dinero desde otro sistema
    public void EstablecerDinero(int cantidad)
    {
        dinero = cantidad;
        ActualizarUI();
    }

    public int ObtenerDinero() => dinero;

}
