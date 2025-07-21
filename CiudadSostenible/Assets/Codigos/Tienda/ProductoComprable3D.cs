using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ProductoComprable3D : MonoBehaviour
{
    [Header("Datos del producto")]
    public ItemData itemData;
    public int precio;

    [Header("Requisito adicional")]
    public ItemData itemRequerido;
    public int cantidadRequerida;

    [Header("Tipo de producto")]
    public bool esMejora = false;
    public GameObject objetoAntiguo;
    public GameObject objetoNuevo;

    public bool esDecorativo = false;
    public GameObject objetoDecorativo;

    [Header("Overlay visual")]
    public GameObject outlineMesh;
    public GameObject panelFlotante;

    [Header("UI detalle compartido")]
    public GameObject panelDetalle;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoPrecio;
    public TextMeshProUGUI textoDescripcion;
    public Image imagenIcono;
    public Button botonComprar;
    public Button botonCancelar;

    [TextArea]
    public string descripcionPersonalizada;

    [Header("Manager de tienda")]
    public TiendaComprasUI tienda;

    private InventorySystem inventario;
    private bool isMouseOver = false;
    private Coroutine parpadeoRojo;

    void Start()
    {
        inventario = tienda.inventario;

        outlineMesh?.SetActive(false);
        panelFlotante?.SetActive(false);
        panelDetalle?.SetActive(false);
    }

    void OnMouseEnter()
    {
        if (isMouseOver) return;

        isMouseOver = true;
        outlineMesh?.SetActive(true);
        panelFlotante?.SetActive(true);
    }

    void OnMouseExit()
    {
        if (!isMouseOver) return;

        isMouseOver = false;
        outlineMesh?.SetActive(false);
        panelFlotante?.SetActive(false);
    }

    void OnMouseDown()
    {
        MostrarPanelDetalle();
    }

    void MostrarPanelDetalle()
    {
        if (panelDetalle == null) return;

        textoNombre.text = itemData.itemName;
        textoPrecio.text = "$" + precio;
        textoDescripcion.text = descripcionPersonalizada;
        imagenIcono.sprite = itemData.icon;

        panelDetalle.SetActive(true);
    }

    public void Comprar()
    {
        bool tieneDinero = tienda.TieneDineroSuficiente(precio);
        bool tieneMateriales = inventario.TieneItem(itemRequerido, cantidadRequerida);

        // Validaciones previas
        if (!tieneDinero && !tieneMateriales)
        {
            tienda.MostrarMensaje("No tienes dinero ni materiales suficientes.");
            ParpadearBotonRojo();
            return;
        }
        else if (!tieneDinero)
        {
            tienda.MostrarMensaje("No tienes dinero suficiente.");
            ParpadearBotonRojo();
            return;
        }
        else if (!tieneMateriales)
        {
            tienda.MostrarMensaje($"Faltan materiales: {itemRequerido.itemName}");
            ParpadearBotonRojo();
            return;
        }

        // MEJORA
        if (esMejora)
        {
            tienda.RestarDinero(precio);
            inventario.RemoveItem(itemRequerido, cantidadRequerida);
            objetoAntiguo?.SetActive(false);
            objetoNuevo?.SetActive(true);
            panelDetalle?.SetActive(false);
        }
        // DECORATIVO
        else if (esDecorativo)
        {
            tienda.RestarDinero(precio);
            inventario.RemoveItem(itemRequerido, cantidadRequerida);
            objetoDecorativo?.SetActive(true);
            panelDetalle?.SetActive(false);
        }
        // ÍTEM NORMAL
        else
        {
            bool añadido = tienda.AnadirAlInventario(itemData);
            if (añadido)
            {
                tienda.RestarDinero(precio);
                inventario.RemoveItem(itemRequerido, cantidadRequerida);
                panelDetalle?.SetActive(false);
            }
            else
            {
                tienda.MostrarMensaje("¡No hay espacio en el inventario!");
                ParpadearBotonRojo();
                return;
            }
        }
    }

    private void ParpadearBotonRojo()
    {
        if (parpadeoRojo != null) StopCoroutine(parpadeoRojo);
        parpadeoRojo = StartCoroutine(ParpadeoBotonRojo());
    }

    private IEnumerator ParpadeoBotonRojo()
    {
        Image img = botonComprar.image != null ? botonComprar.image : botonComprar.GetComponent<Image>();
        Color original = img.color;
        img.color = Color.red;
        yield return new WaitForSeconds(0.25f);
        img.color = original;
    }

    public void CerrarPanelDetalle()
    {
        panelDetalle?.SetActive(false);
    }

}
