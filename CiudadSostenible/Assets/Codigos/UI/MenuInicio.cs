using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuInicio : MonoBehaviour
{
    [Header("ELEMENTOS DEL MENÚ")]
    public GameObject[] elementosMenu;

    [Header("CONFIGURACIÓN ANIMACIÓN")]
    public float duracionFadeIn = 1.2f;
    public float delayEntreElementos = 0.25f;
    public bool animarEscala = true;
    public int porcentajeEscalaFinal = 100;
    public int escena;

    [Header("PANTALLA DE CARGA")]
    public GameObject canvasPrincipal;
    public GameObject canvasCarga;
    public PantallaCarga pantallaCarga;

    private Vector3[] escalasOriginales;

    void Start()
    {
        if (elementosMenu == null || elementosMenu.Length == 0)
        {
            Debug.LogError("No hay elementos asignados en el menú!", this);
            enabled = false;
            return;
        }

        canvasPrincipal.SetActive(true);
        if (canvasCarga != null) canvasCarga.SetActive(false);

        escalasOriginales = new Vector3[elementosMenu.Length];

        for (int i = 0; i < elementosMenu.Length; i++)
        {
            if (elementosMenu[i] != null)
            {
                escalasOriginales[i] = elementosMenu[i].transform.localScale;
                SetAlpha(elementosMenu[i], 0f);

                if (animarEscala)
                {
                    elementosMenu[i].transform.localScale = escalasOriginales[i] * 0.7f;
                }
            }
            else
            {
                Debug.LogWarning($"Elemento {i} del menú no está asignado!", this);
            }
        }

        StartCoroutine(AnimarEntrada());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SalirDelJuego();
        }
        else if (Input.anyKeyDown)
        {
            IniciarPantallaCarga();
        }
    }

    IEnumerator AnimarEntrada()
    {
        for (int i = 0; i < elementosMenu.Length; i++)
        {
            if (elementosMenu[i] != null)
            {
                StartCoroutine(AnimarElemento(elementosMenu[i], i));
                yield return new WaitForSeconds(delayEntreElementos);
            }
        }
    }

    IEnumerator AnimarElemento(GameObject obj, int index)
    {
        float tiempo = 0f;
        Vector3 escalaFinal = escalasOriginales[index] * (porcentajeEscalaFinal / 100f);
        Vector3 escalaInicial = animarEscala ? escalasOriginales[index] * 0.7f : escalasOriginales[index];

        // Componentes para fade
        Renderer renderer = obj.GetComponent<Renderer>();
        TextMesh textMesh = obj.GetComponent<TextMesh>();

        while (tiempo < duracionFadeIn)
        {
            tiempo += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempo / duracionFadeIn);

            // Fade In
            if (renderer != null) renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, progreso);
            if (textMesh != null) textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, progreso);

            if (animarEscala)
            {
                obj.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, progreso);
            }

            yield return null;
        }

        // valores finales exactos
        if (renderer != null) renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 1f);
        if (textMesh != null) textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 1f);

        if (animarEscala)
        {
            obj.transform.localScale = escalaFinal;
        }
    }

    void SetAlpha(GameObject obj, float alpha)
    {
        if (obj == null) return;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;
        }

        TextMesh textMesh = obj.GetComponent<TextMesh>();
        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = alpha;
            textMesh.color = color;
        }
    }

    void IniciarJuego()
    {
        StopAllCoroutines();
        SceneManager.LoadScene(escena);
    }

    public void IniciarPantallaCarga()
    {
        StopAllCoroutines();

        // Desactivar menú principal y activar pantalla de carga
        canvasPrincipal.SetActive(false);
        if (canvasCarga != null)
        {
            canvasCarga.SetActive(true);

            PantallaCarga pantalla = canvasCarga.GetComponent<PantallaCarga>();

            // Iniciar la secuencia de carga
            if (pantallaCarga != null)
            {
                pantallaCarga.IniciarSecuenciaCarga(escena);
            }
            else
            {
                Debug.LogWarning("No se asignó el componente PantallaCarga! Cargando escena directamente...");
                SceneManager.LoadScene(escena);
            }
        }
        else
        {
            Debug.LogWarning("No se asignó el Canvas de carga! Cargando escena directamente...");
            SceneManager.LoadScene(escena);
        }
    }

    void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}