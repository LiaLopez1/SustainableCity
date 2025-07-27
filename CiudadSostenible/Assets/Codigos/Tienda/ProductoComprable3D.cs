using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider))]
public class ProductoComprable3D : MonoBehaviour
{
    [HideInInspector] public bool desbloqueado = false;

    [Header("Interacción y Hover")]
    public Camera cameraInteractiva;      // Tu cámara principal
    public LayerMask shopItemLayer;       // Layer “ShopItem” para objetos de tienda

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
    public TiendaComprasUI tienda;       // Manager central de la tienda

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
        // Si está bloqueado, no permitimos interacción
        if (!desbloqueado) return;

        // Manejo de overlay y hover
        if (tienda != null && tienda.panelDetalle != null && tienda.panelDetalle.activeInHierarchy)
        {
            if (isHovered)
            {
                isHovered = false;
                outlineMesh?.SetActive(false);
                panelFlotante?.SetActive(false);
            }
            return;
        }

        // 1) Si la cámara no está activa, abortamos
        if (cameraInteractiva == null || !cameraInteractiva.gameObject.activeInHierarchy)
        {
            if (isHovered)
            {
                isHovered = false;
                outlineMesh?.SetActive(false);
                panelFlotante?.SetActive(false);
            }
            return;
        }

        // 2) Raycast desde el cursor
        Ray ray = cameraInteractiva.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool ahoraHover = Physics.Raycast(ray, out hit, Mathf.Infinity, shopItemLayer)
                          && hit.collider.GetComponentInParent<ProductoComprable3D>() == this;

        // 3) Mostrar/ocultar outline & panel flotante
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

        // 4) Clic para abrir detalle
        if (ahoraHover && Input.GetMouseButtonDown(0))
        {
            tienda.AbrirDetalle(this);
        }
    }

    /// <summary>
    /// Invocado por TiendaComprasUI.ComprarActual()
    /// </summary>
    public void Comprar()
    {
        // Si está bloqueado, mostramos mensaje y salimos
        if (!desbloqueado)
        {
            tienda.MostrarMensaje("Este objeto está bloqueado.");
            return;
        }

        bool tieneDinero = tienda.TieneDineroSuficiente(precio);
        bool tieneMateriales = inventario.TieneItem(itemRequerido, cantidadRequerida);

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

        // Lógica de compra intacta
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
        bool anadido = tienda.AnadirAlInventario(itemData);
        if (anadido)
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
