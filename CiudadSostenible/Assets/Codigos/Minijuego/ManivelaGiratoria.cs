using UnityEngine;

public class ManivelaGiratoria : MonoBehaviour
{
    [Header("Referencias")]
    public Transform manivelaVisual;
    public Camera camara;

    // Enum para seleccionar el tipo de objeto en el Inspector
    public enum TipoDeObjeto
    {
        Objeto1 = 1,  // Primer objeto con PlasticBowlCounter
        Objeto2 = 2   // Segundo objeto con SecondPlasticBowlCounter
    }

    // Selección de tipo de objeto desde el Inspector
    [Header("Configuración")]
    public TipoDeObjeto tipoDeObjeto = TipoDeObjeto.Objeto1; // Valor predeterminado, puedes cambiarlo en el Inspector

    // Variables para almacenar los dos posibles contadores
    public PlasticBowlCounter bowlCounter;
    public SecondPlasticBowlCounter secondBowlCounter;

    public float sensibilidad = 1f;
    public float vueltaCompleta = 360f;

    private bool girando = false;
    private float ultimoAngulo;
    private Vector3 centroPantalla;
    private float rotacionAcumulada = 0f;

    void Start()
    {
        // Asignar el componente correcto según el valor de tipoDeObjeto
        if (tipoDeObjeto == TipoDeObjeto.Objeto1)
        {
            bowlCounter = FindObjectOfType<PlasticBowlCounter>();
        }
        else if (tipoDeObjeto == TipoDeObjeto.Objeto2)
        {
            secondBowlCounter = FindObjectOfType<SecondPlasticBowlCounter>();
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

                    // Dependiendo del tipo de objeto seleccionado, ejecutar el código correspondiente
                    if (tipoDeObjeto == TipoDeObjeto.Objeto1 && bowlCounter != null)
                    {
                        bowlCounter.ProcesarUnaBotellaDirecto();
                    }
                    else if (tipoDeObjeto == TipoDeObjeto.Objeto2 && secondBowlCounter != null)
                    {
                        secondBowlCounter.ProcesarUnaBotellaDirecto();
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
