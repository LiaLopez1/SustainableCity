using UnityEngine;

public class CamaraJugador : MonoBehaviour
{
    public Transform target; // Asigna tu personaje en el Inspector.
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0, 5f, -5f); // Ajusta Y y Z seg�n tu escena.
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // Configura la rotaci�n inicial de la c�mara (�ngulo fijo).
        transform.rotation = Quaternion.Euler(30f, 0f, 0f); // Ajusta el 30� seg�n tu juego.
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Sigue al personaje en X, Y y Z.
            Vector3 targetPosition = target.position + offset;

            // Suaviza el movimiento (sin bloquear ejes).
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // �No uses LookAt! La rotaci�n ya est� fijada en Start().
        }
    }
}