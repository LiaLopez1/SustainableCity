
using UnityEngine;

public class ManivelaGiratoria : MonoBehaviour
{
    [Header("Referencias")]
    public Transform manivelaVisual;
    public Camera camara;
    public PlasticBowlCounter bowlCounter;

    [Header("Configuración")]
    public float sensibilidad = 1f;
    public float vueltaCompleta = 360f;

    private bool girando = false;
    private float ultimoAngulo;
    private Vector3 centroPantalla;
    private float rotacionAcumulada = 0f;

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

            // Solo permitir giro hacia la izquierda (negativo)
            if (delta < 0)
            {
                float rotacion = -delta * sensibilidad;
                manivelaVisual.Rotate(Vector3.right, rotacion);
                rotacionAcumulada += Mathf.Abs(rotacion);

                if (rotacionAcumulada >= vueltaCompleta)
                {
                    rotacionAcumulada = 0f;
                    Debug.Log("🔄 Vuelta completa detectada en sentido izquierdo.");

                    if (bowlCounter != null)
                    {
                        bowlCounter.ProcesarUnaBotella();
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
