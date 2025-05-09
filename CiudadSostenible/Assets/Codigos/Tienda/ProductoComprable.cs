using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ProductoComprable : MonoBehaviour
{
    [Header("Datos del Producto")]
    public ItemData itemData; // Ítem que se entrega al comprar
    public int precio;

    [Header("Requisito adicional")]
    public ItemData itemRequerido;     // Ítem necesario para comprar (ej: plástico)
    public int cantidadRequerida;      // Cantidad necesaria de ese ítem

    [Header("Referencias UI")]
    public Button botonComprar;
    public TextMeshProUGUI textoPrecio;
    public TextMeshProUGUI textoRequisito; // "Requiere: 5 Plástico"
    public TiendaComprasUI tienda;

    [Header("Mejoras de mundo (opcional)")]
    public GameObject objetoAntiguo; // Máquina vieja
    public GameObject objetoNuevo;  // Máquina mejorada
    public bool esMejora = false;   // Si este producto es una mejora, no un ítem

    [Header("Decoración (opcional)")]
    public bool esDecorativo = false;
    public GameObject objetoDecorativo; // El objeto que se activa al comprar

    private InventorySystem inventario;
    private Image imagenBoton;
    private Color colorOriginal;


    void Start()
    {
        imagenBoton = botonComprar.GetComponent<Image>();

        if (itemData == null || itemRequerido == null || tienda == null)
        {
            Debug.LogError($"Faltan referencias en {gameObject.name}");
            botonComprar.interactable = false;
            return;
        }

        // Mostrar precio y requisito
        imagenBoton.sprite = itemData.icon;
        textoPrecio.text = "$" + precio;
        textoRequisito.text = $"{cantidadRequerida} {itemRequerido.itemName}";

        colorOriginal = imagenBoton.color;

        inventario = tienda.inventario;
        botonComprar.onClick.AddListener(Comprar);
    }

    public void Comprar()
    {
        if (!tienda.TieneDineroSuficiente(precio))
        {
            tienda.MostrarMensaje("Not enough money!");
            StartCoroutine(EfectoEspasmoRojo());
            return;
        }

        if (!inventario.TieneItem(itemRequerido, cantidadRequerida))
        {
            tienda.MostrarMensaje($" {itemRequerido.itemName} Missing!");
            StartCoroutine(EfectoEspasmoRojo());
            return;
        }

        tienda.RestarDinero(precio);
        inventario.RemoveItem(itemRequerido, cantidadRequerida);

        if (esMejora)
        {
            if (objetoAntiguo != null) objetoAntiguo.SetActive(false);
            if (objetoNuevo != null) objetoNuevo.SetActive(true);
        }
        else if (esDecorativo)
        {
            if (objetoDecorativo != null) objetoDecorativo.SetActive(true);
        }
        else
        {
            bool añadido = tienda.AnadirAlInventario(itemData);

            if (!añadido)
            {
                tienda.MostrarMensaje("No space in inventory!");
                StartCoroutine(EfectoEspasmoRojo());
            }
        }
    }



    private IEnumerator EfectoEspasmoRojo()
    {
        imagenBoton.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        imagenBoton.color = colorOriginal;
    }
}
