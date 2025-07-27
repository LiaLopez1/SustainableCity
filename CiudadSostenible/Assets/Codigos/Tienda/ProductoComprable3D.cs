using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ProductoComprable3D : MonoBehaviour
{
    [Header("Interacción y Hover")]
    public Camera cameraInteractiva;      // Tu cámara principal
    public LayerMask shopItemLayer;       // Layer “ShopItem” para tus objetos de tienda

    [Header("Datos del producto")]
    public ItemData itemData;
    public int precio;

    [Header("Requisito adicional")]
    public ItemData itemRequerido;
    public int cantidadRequerida;

    [Header("Tipo de producto")]
    public bool esMejora;
    public GameObject objetoAntiguo;
    public GameObject objetoNuevo;

    public bool esDecorativo;
    public GameObject objetoDecorativo;

    [Header("Overlay visual")]
    public GameObject outlineMesh;        // Malla duplicada con shader outline
    public GameObject panelFlotante;      // Panel pequeño con nombre+precio

    [TextArea]
    public string descripcionPersonalizada;

    [Header("Manager de tienda")]
    public TiendaComprasUI tienda;       // Referencia a tu manager central

    private InventorySystem inventario;
    private bool isHovered;

    void Start()
    {
        inventario = tienda.inventario;
        outlineMesh?.SetActive(false);
        panelFlotante?.SetActive(false);
    }

    void Update()
    {
        // 1) Raycast desde el cursor
        Ray ray = cameraInteractiva.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool ahoraHover = Physics.Raycast(ray, out hit, Mathf.Infinity, shopItemLayer)
                          && hit.collider.GetComponentInParent<ProductoComprable3D>() == this;

        // 2) Mostrar/ocultar outline & panel flotante
        if (ahoraHover && !isHovered)
        {
            isHovered = true;
            outlineMesh?.SetActive(true);
            panelFlotante?.SetActive(true);
        }
        else if (!ahoraHover && isHovered)
        {
            isHovered = false;
            outlineMesh?.SetActive(false);
            panelFlotante?.SetActive(false);
        }

        // 3) Clic para abrir detalle en el manager
        if (ahoraHover && Input.GetMouseButtonDown(0))
        {
            tienda.AbrirDetalle(this);
        }
    }

    /// <summary>
    /// Llamado por TiendaComprasUI.ComprarActual()
    /// </summary>
    public void Comprar()
    {
        bool tieneDinero = tienda.TieneDineroSuficiente(precio);
        bool tieneMateriales = inventario.TieneItem(itemRequerido, cantidadRequerida);

        // Validaciones
        if (!tieneDinero && !tieneMateriales)
        {
            tienda.MostrarMensaje("No tienes dinero ni materiales suficientes.");
            return;
        }
        if (!tieneDinero)
        {
            tienda.MostrarMensaje("No tienes dinero suficiente.");
            return;
        }
        if (!tieneMateriales)
        {
            tienda.MostrarMensaje($"Faltan materiales: {itemRequerido.itemName}");
            return;
        }

        // Lógica de compra
        if (esMejora)
        {
            tienda.RestarDinero(precio);
            inventario.RemoveItem(itemRequerido, cantidadRequerida);
            objetoAntiguo?.SetActive(false);
            objetoNuevo?.SetActive(true);
            tienda.CerrarDetalle();
            return;
        }
        if (esDecorativo)
        {
            tienda.RestarDinero(precio);
            inventario.RemoveItem(itemRequerido, cantidadRequerida);
            objetoDecorativo?.SetActive(true);
            tienda.CerrarDetalle();
            return;
        }
        bool añadido = tienda.AnadirAlInventario(itemData);
        if (añadido)
        {
            tienda.RestarDinero(precio);
            inventario.RemoveItem(itemRequerido, cantidadRequerida);
            tienda.CerrarDetalle();
        }
        else
        {
            tienda.MostrarMensaje("¡No hay espacio en el inventario!");
        }
    }
}
