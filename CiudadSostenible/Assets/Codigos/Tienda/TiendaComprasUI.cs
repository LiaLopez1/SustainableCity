using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TiendaComprasUI : MonoBehaviour
{
    [Header("Referencias Inventario y Dinero")]
    public InventorySystem inventario;
    public TextMeshProUGUI textoDinero;
    public TextMeshProUGUI textoAdvertencia;

    [Header("Panel de detalle")]
    public GameObject panelDetalle;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoPrecio;
    public TextMeshProUGUI textoDescripcion;
    public Image imagenIcono;
    public Button botonComprar;
    public Button botonCancelar;

    // Estado interno
    private ProductoComprable3D productoActual;

    // Para el parpadeo
    private Coroutine blinkCoroutine;
    private Color originalBuyColor;

    void Awake()
    {
        // Guardamos color original del botón
        if (botonComprar != null && botonComprar.image != null)
            originalBuyColor = botonComprar.image.color;

        // Configuramos eventos de los botones
        botonComprar.onClick.RemoveAllListeners();
        botonComprar.onClick.AddListener(ComprarActual);

        botonCancelar.onClick.RemoveAllListeners();
        botonCancelar.onClick.AddListener(CerrarDetalle);

        // Panel de detalle oculto al inicio
        panelDetalle.SetActive(false);
    }

    /// <summary>
    /// Muestra un mensaje de advertencia y hace parpadear el botón de comprar.
    /// </summary>
    public void MostrarMensaje(string mensaje)
    {
        Debug.Log("MENSAJE: " + mensaje);

        if (textoAdvertencia != null)
        {
            textoAdvertencia.text = mensaje;
            textoAdvertencia.gameObject.SetActive(true);

            // Blink del botón
            ParpadearBotonRojo();

            // Ocultar mensaje tras 2s
            CancelInvoke(nameof(OcultarMensaje));
            Invoke(nameof(OcultarMensaje), 2f);
        }
    }

    private void OcultarMensaje()
    {
        textoAdvertencia.gameObject.SetActive(false);
    }

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

    /// <summary>
    /// Llamado por un ProductoComprable3D cuando se hace clic.
    /// </summary>
    public void AbrirDetalle(ProductoComprable3D producto)
    {
        productoActual = producto;

        textoNombre.text = producto.itemData.itemName;
        textoPrecio.text = "$" + producto.precio;
        textoDescripcion.text = producto.descripcionPersonalizada;
        imagenIcono.sprite = producto.itemData.icon;

        panelDetalle.SetActive(true);
    }

    /// <summary>
    /// Llamado por el botón “Comprar”.
    /// </summary>
   public void ComprarActual()
    {
        if (productoActual != null)
            productoActual.Comprar();
    }

    /// <summary>
    /// Llamado por el botón “Cancelar” o al éxito de la compra.
    /// </summary>
    public void CerrarDetalle()
    {
        panelDetalle.SetActive(false);
        productoActual = null;
    }

    /// <summary>
    /// Inicia el parpadeo en rojo del botón Comprar.
    /// </summary>
    private void ParpadearBotonRojo()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        if (botonComprar == null || botonComprar.image == null)
            yield break;

        var img = botonComprar.image;
        img.color = Color.red;
        yield return new WaitForSeconds(0.25f);
        img.color = originalBuyColor;
    }

    /// <summary>
    /// (Opcional) Para actualizar el texto de dinero desde otros sistemas.
    /// </summary>
    public void EstablecerDinero(int cantidad)
    {
        if (textoDinero != null)
            textoDinero.text = "$" + cantidad;
    }
}
