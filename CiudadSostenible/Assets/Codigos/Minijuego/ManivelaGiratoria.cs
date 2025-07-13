using UnityEngine;

public class ManivelaGiratoria : MonoBehaviour
{
    [Header("Referencias")]
    public Transform manivelaVisual;
    public Camera camara;

    [Header("Configuración")]
    public PlasticBowlCounter bowlCounter;

    public float sensibilidad = 1f;
    public float vueltaCompleta = 360f;

    private bool girando = false;
    private float ultimoAngulo;
    private Vector3 centroPantalla;
    private float rotacionAcumulada = 0f;

    void Start()
    {
        // Asignar automáticamente el primer PlasticBowlCounter en la escena si no se asigna desde el inspector
        if (bowlCounter == null)
        {
            bowlCounter = FindObjectOfType<PlasticBowlCounter>();
        }
    }

    void OnMouseDown()
    {
        girando = true;
        Vector3 screenPos = camara.WorldToScreenPoint(transform.position);
        centroPantalla = new Vector3(screenPos.x, screenPos.y, 0f);
        ultimoAngulo = ObtenerAngulo(Input.mousePosition);
    }

    void OnMouseUp()
    {
        girando = false;
    }

    void Update()
    {
        if (girando)
        {
            float nuevoAngulo = ObtenerAngulo(Input.mousePosition);
            float delta = Mathf.DeltaAngle(ultimoAngulo, nuevoAngulo);

            // Solo permitir giro hacia la izquierda
            if (delta < 0)
            {
                float rotacion = -delta * sensibilidad;
                manivelaVisual.Rotate(Vector3.right, rotacion);
                rotacionAcumulada += Mathf.Abs(rotacion);

                if (rotacionAcumulada >= vueltaCompleta)
                {
                    rotacionAcumulada = 0f;

                    if (bowlCounter != null)
                    {
                        bowlCounter.ProcesarUnaBotellaDirecto();
                    }
                    else
                    {
                        Debug.LogWarning("❌ No se encontró el contenedor adecuado.");
                    }
                }
            }

            ultimoAngulo = nuevoAngulo;
        }
    }

    float ObtenerAngulo(Vector3 posicionMouse)
    {
        Vector3 direccion = posicionMouse - centroPantalla;
        return Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
    }
}
