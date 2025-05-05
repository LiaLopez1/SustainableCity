using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CamionFullCapacity : MonoBehaviour
{
    [Header("Configuración")]
    public string targetTag = "NoAprovechables";
    public int capacidadMaxima = 7;
    public TextMeshProUGUI mensajeTMP;

    [Header("Movimiento del camión")]
    public Transform objetoPadreAMover;
    public float desplazamientoEnX = 10f;
    public float velocidadMovimiento = 0.2f;

    [Header("Cambio de cámara")]
    public Camera mainCamera;
    public Canvas specialCanvas;
    public PlayerMove playerMovement;

    [Header("Estado actual")]
    private HashSet<GameObject> bolsasDentro = new HashSet<GameObject>();
    private Vector3 posicionInicial;
    public bool EstaLleno => bolsasDentro.Count >= capacidadMaxima;

    private InventoryItemDragHandler dragHandler;

    private void Start()
    {
        if (objetoPadreAMover != null)
            posicionInicial = objetoPadreAMover.position;

        dragHandler = FindObjectOfType<InventoryItemDragHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            GameObject raiz = other.transform.root.gameObject;

            if (!bolsasDentro.Contains(raiz))
            {
                bolsasDentro.Add(raiz);

                // Hacer hija del camión
                raiz.transform.SetParent(this.transform, true);

                if (bolsasDentro.Count >= capacidadMaxima)
                {
                    MostrarMensaje();
                    StartCoroutine(PrepararSalidaCamion());
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            GameObject raiz = other.transform.root.gameObject;

            if (bolsasDentro.Contains(raiz))
            {
                bolsasDentro.Remove(raiz);
                raiz.transform.SetParent(null, true);
            }
        }
    }

    private void MostrarMensaje()
    {
        if (mensajeTMP != null)
        {
            mensajeTMP.gameObject.SetActive(true);
            StartCoroutine(OcultarMensajeDespuesDe(3f));
        }
    }

    private IEnumerator OcultarMensajeDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (mensajeTMP != null)
        {
            mensajeTMP.gameObject.SetActive(false);
        }
    }

    private IEnumerator PrepararSalidaCamion()
    {
        yield return new WaitForSeconds(2f); // Delay antes de arrancar

        yield return StartCoroutine(MoverCamion());

        CambiarACamaraPrincipal();

        // Destruir hijos con tag "NoAprovechables"
        List<Transform> hijosParaDestruir = new List<Transform>();
        foreach (Transform hijo in transform)
        {
            if (hijo.CompareTag("NoAprovechables"))
            {
                hijosParaDestruir.Add(hijo);
            }
        }

        foreach (Transform hijo in hijosParaDestruir)
        {
            Destroy(hijo.gameObject);
        }

        bolsasDentro.Clear();

        // Esperar 1 minuto
        yield return new WaitForSeconds(60f);

        // Volver a posición original
        yield return StartCoroutine(RetornarAPosicionInicial());

        // Reiniciar lógica de filas
        if (dragHandler != null)
        {
            dragHandler.ResetearPosicionNoAprovechables();
        }
    }

    private IEnumerator MoverCamion()
    {
        if (objetoPadreAMover == null) yield break;

        Vector3 destino = objetoPadreAMover.position + new Vector3(desplazamientoEnX, 0f, 0f);
        Vector3 inicio = objetoPadreAMover.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadMovimiento;
            objetoPadreAMover.position = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }
    }

    private IEnumerator RetornarAPosicionInicial()
    {
        if (objetoPadreAMover == null) yield break;

        Vector3 destino = posicionInicial;
        Vector3 inicio = objetoPadreAMover.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadMovimiento;
            objetoPadreAMover.position = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }
    }

    private void CambiarACamaraPrincipal()
    {
        if (mainCamera != null)
            mainCamera.enabled = true;

        if (specialCanvas != null)
            specialCanvas.gameObject.SetActive(false);

        if (playerMovement != null)
            playerMovement.EnableMovement(true);
    }
}
