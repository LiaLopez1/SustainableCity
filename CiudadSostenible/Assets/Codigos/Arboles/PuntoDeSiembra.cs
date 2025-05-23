﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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

    [Header("UI de retroalimentación")]
    public TextMeshProUGUI feedbackTexto;

    [Header("ItemData necesarios")]
    public ItemData semillaItem;
    public ItemData compostaItem;
    public ItemData bidonLlenoItem;
    public ItemData bidonVacioItem;

    [Header("Duraciones personalizables")]
    public float duracionDrenaje = 150f; // En segundos
    public float duracionCrecimiento = 300f; // En segundos

    private bool jugadorDentro = false;
    private bool sembrado = false;
    private bool regado = false;

    private InventorySystem inventario;
    private Coroutine drenajeAguaCoroutine;
    private GameObject arbolInstanciado;

    private Coroutine crecimientoCoroutine;
    private Coroutine feedbackCoroutine;

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
            textoRegar?.SetActive(sembrado && sliderAgua.value < 1f);
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
            {
                monticuloObject.SetActive(true);
                MostrarFeedback("Seed planted. Add compost to continue.");
            }
            else
            {
                MostrarFeedback("You need a seed to plant.");
            }
        }
        else
        {
            // Ya hay montículo, intentamos compostar
            if (arbolInstanciado == null && arbolPrefab != null && spawnPoint != null)
            {
                quitadoComposta = inventario.RemoveItem(compostaItem, 1);
                if (quitadoComposta)
                {
                    arbolInstanciado = Instantiate(arbolPrefab, spawnPoint.position, spawnPoint.rotation);
                    MostrarFeedback("Compost added. Now water the tree.");
                }
                else
                {
                    MostrarFeedback("You need compost to continue.");
                }
            }
        }

        // Si el jugador tenía ambos desde el inicio
        if (monticuloObject.activeSelf && arbolInstanciado == null)
        {
            bool tieneComposta = inventario.RemoveItem(compostaItem, 1);
            if (tieneComposta)
            {
                arbolInstanciado = Instantiate(arbolPrefab, spawnPoint.position, spawnPoint.rotation);
                MostrarFeedback("Tree planted. Now water it.");
            }
        }

        // Si ya se completaron semilla y composta
        if (arbolInstanciado != null)
        {
            sembrado = true;
            sliderAgua.gameObject.SetActive(true);
            sliderAgua.value = 0;
        }
    }


    void MostrarFeedback(string mensaje)
    {
        if (feedbackTexto == null) return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackTexto.text = mensaje;
        feedbackTexto.gameObject.SetActive(true);
        feedbackCoroutine = StartCoroutine(EsconderFeedback());
    }

    IEnumerator EsconderFeedback()
    {
        yield return new WaitForSeconds(2.5f);
        feedbackTexto.gameObject.SetActive(false);
    }

    void IntentarRegar()
    {
        foreach (var slot in inventario.slots)
        {
            ItemData item = slot.GetItemData();
            if (item == bidonLlenoItem)
            {
                sliderAgua.value = 1f;

                if (drenajeAguaCoroutine != null)
                    StopCoroutine(drenajeAguaCoroutine);

                if (crecimientoCoroutine != null)
                    StopCoroutine(crecimientoCoroutine);

                drenajeAguaCoroutine = StartCoroutine(DrenarAgua());
                crecimientoCoroutine = StartCoroutine(EsperarCrecimiento());

                inventario.RemoveItem(bidonLlenoItem, 1);
                inventario.AddItem(bidonVacioItem, 1);

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
        yield return new WaitForSeconds(duracionCrecimiento);

        if (sliderAgua.value >= 0.5f && arbolInstanciado != null)
        {
            arbolInstanciado.transform.localScale = new Vector3(25f, 25f, 25f);
            sliderAgua.gameObject.SetActive(false); // 👈 Oculta el slider
            Debug.Log("🌳 ¡El árbol ha crecido completamente!");
        }

    }


}
