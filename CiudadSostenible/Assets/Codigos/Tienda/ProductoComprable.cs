using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ProductoComprable : MonoBehaviour
{
    [Header("Datos del Producto")]
    public ItemData itemData;
    public int precio;
    public int basuraRequerida;

    [Header("Referencias UI")]
    public Button botonComprar;
    public TextMeshProUGUI textoPrecio;
    public TextMeshProUGUI textoProgresoBasura; // muestra "x / basuraRequerida"
    public TiendaComprasUI tienda;

    private Color colorOriginal;
    private BasuraSpawner spawner;
    private Image imagenBoton;

    void Start()
    {
        imagenBoton = botonComprar.GetComponent<Image>(); // ¡NECESARIA!

        if (itemData == null)
        {
            Debug.LogError($"ItemData no asignado en {gameObject.name}");
            botonComprar.interactable = false;
            return;
        }

        imagenBoton.sprite = itemData.icon;
        textoPrecio.text = "$" + precio;
        colorOriginal = imagenBoton.color;

        spawner = FindObjectOfType<BasuraSpawner>();
        botonComprar.onClick.AddListener(Comprar);

        ActualizarDisponibilidad();
    }



    void Update()
    {
        ActualizarDisponibilidad(); // chequea en tiempo real
    }

    void ActualizarDisponibilidad()
    {
        if (spawner == null) return;

        int basuraRecogida = spawner.GetBasuraRecogida();
        textoProgresoBasura.text = $"{basuraRecogida} / {basuraRequerida}";

        bool desbloqueado = basuraRecogida >= basuraRequerida;

        botonComprar.interactable = desbloqueado;

        // Visual feedback: desaturar ítem si está bloqueado
        imagenBoton.color = desbloqueado ? colorOriginal : new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    public void Comprar()
    {
        if (tienda == null || itemData == null) return;

        if (tienda.TieneDineroSuficiente(precio))
        {
            bool añadido = tienda.AñadirAlInventario(itemData);

            if (añadido)
            {
                tienda.RestarDinero(precio);
            }
            else
            {
                tienda.MostrarMensaje("¡No hay espacio en el inventario!");
                StartCoroutine(EfectoEspasmoRojo());
            }
        }
        else
        {
            tienda.MostrarMensaje("¡Dinero insuficiente!");
            StartCoroutine(EfectoEspasmoRojo());
        }
    }

    private IEnumerator EfectoEspasmoRojo()
    {
        imagenBoton.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        imagenBoton.color = colorOriginal;
    }
}
