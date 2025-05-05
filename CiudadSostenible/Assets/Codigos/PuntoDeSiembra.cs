using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static UnityEditor.Progress;

public class PuntoDeSiembraSimple : MonoBehaviour
{
    [Header("Referencias a objetos en escena")]
    public GameObject monticuloObject;          // Ya en escena, desactivado
    public GameObject arbolPrefab;              // Se instancia
    public Transform spawnPoint;                // Lugar donde aparece el árbol
    public Slider sliderAgua;

    [Header("UI de interacción")]
    public GameObject textoPlantar;
    public GameObject textoRegar;

    [Header("ItemData necesarios")]
    public ItemData semillaItem;
    public ItemData compostaItem;
    public ItemData bidonItem;

    private bool jugadorDentro = false;
    private bool sembrado = false;
    private bool regado = false;

    private InventorySystem inventario;
    private Coroutine drenajeAguaCoroutine;
    private GameObject arbolInstanciado;

    private const float duracionDrenaje = 150f;
    private Coroutine crecimientoCoroutine;

    void Start()
    {
        inventario = FindObjectOfType<InventorySystem>();
        sliderAgua.gameObject.SetActive(false);
        textoPlantar?.SetActive(false);
        textoRegar?.SetActive(false);
        monticuloObject?.SetActive(false);
    }

    void Update()
    {
        if (jugadorDentro && Input.GetKeyDown(KeyCode.E))
        {
            if (!sembrado)
                IntentarSembrar();
            else if (!regado)
                IntentarRegar();
        }

        if (jugadorDentro)
        {
            textoPlantar?.SetActive(!sembrado);
            textoRegar?.SetActive(sembrado && !regado);
        }
    }

    void IntentarSembrar()
    {
        bool quitadoSemilla = false;
        bool quitadoComposta = false;

        // Si el montículo no está activo, intentamos plantar la semilla
        if (!monticuloObject.activeSelf)
        {
            quitadoSemilla = inventario.RemoveItem(semillaItem, 1);
            if (quitadoSemilla)
                monticuloObject.SetActive(true);
        }
        else
        {
            // Ya hay montículo, intentamos compostar si no lo habíamos hecho
            if (arbolInstanciado == null && arbolPrefab != null && spawnPoint != null)
            {
                quitadoComposta = inventario.RemoveItem(compostaItem, 1);
                if (quitadoComposta)
                    arbolInstanciado = Instantiate(arbolPrefab, spawnPoint.position, spawnPoint.rotation);

            }
        }

        // Si el jugador tenía ambos desde el inicio
        if (monticuloObject.activeSelf && arbolInstanciado == null)
        {
            bool tieneComposta = inventario.RemoveItem(compostaItem, 1);
            if (tieneComposta)
            {
                arbolInstanciado = Instantiate(arbolPrefab, spawnPoint.position, spawnPoint.rotation);

            }
        }

        // Si se hizo al menos un paso válido
        if (arbolInstanciado != null)
        {
            sembrado = true;
            sliderAgua.gameObject.SetActive(true);
            sliderAgua.value = 0;
        }

        else
        {
            Debug.LogWarning("No tienes semilla o no has sembrado para aplicar composta.");
        }
    }


    void IntentarRegar()
    {
        foreach (var slot in inventario.slots)
        {
            ItemData item = slot.GetItemData();
            if (item == bidonItem && BidonDeAguaManager.Instance.EstaLleno(item))
            {
                BidonDeAguaManager.Instance.VaciarBidon(item);
                sliderAgua.value = 1f;
                regado = true;

                if (drenajeAguaCoroutine != null)
                    StopCoroutine(drenajeAguaCoroutine);

                if (crecimientoCoroutine != null)
                    StopCoroutine(crecimientoCoroutine);

                drenajeAguaCoroutine = StartCoroutine(DrenarAgua());
                crecimientoCoroutine = StartCoroutine(EsperarCrecimiento());

                Debug.Log("🌧 Árbol regado completamente.");
                return;
            }
        }

        Debug.LogWarning("Necesitas un bidón lleno para regar.");
    }


    IEnumerator DrenarAgua()
    {
        float tiempoRestante = duracionDrenaje;
        while (tiempoRestante > 0)
        {
            sliderAgua.value = Mathf.Lerp(0f, 1f, tiempoRestante / duracionDrenaje);
            tiempoRestante -= Time.deltaTime;
            yield return null;
        }

        sliderAgua.value = 0;
        regado = false;
        Debug.Log("💧 Agua drenada. Árbol necesita riego nuevamente.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            jugadorDentro = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            textoPlantar?.SetActive(false);
            textoRegar?.SetActive(false);
        }
    }
    IEnumerator EsperarCrecimiento()
    {
        yield return new WaitForSeconds(300f); // 5 minutos

        if (sliderAgua.value >= 0.5f && arbolInstanciado != null)
        {
            arbolInstanciado.transform.localScale = new Vector3(25f, 25f, 25f);
            Debug.Log("🌳 ¡El árbol ha crecido completamente!");
        }
    }


}
