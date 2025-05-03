using UnityEngine;

public class ManivelaGiratoria : MonoBehaviour
{
    public Transform manivelaVisual;
    public Camera camara;
    public float sensibilidad = 1f;

    private bool girando = false;
    private float ultimoAngulo;
    private Vector3 centroPantalla;

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
            manivelaVisual.Rotate(Vector3.right, -delta * sensibilidad);
            ultimoAngulo = nuevoAngulo;
        }
    }

    float ObtenerAngulo(Vector3 posicionMouse)
    {
        Vector3 direccion = posicionMouse - centroPantalla;
        return Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
    }
}
