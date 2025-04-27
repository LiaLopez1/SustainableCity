using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraJugador : MonoBehaviour
{
    public Transform target; // Asigna tu personaje en el Inspector.
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0, 5f, -5f); // Ajusta Y y Z para perspectiva 3D.
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target != null)
        {
            // Posición objetivo (X, Y, Z) con offset.
            Vector3 targetPosition = target.position + offset;

            // Suavizado (solo en X y Z si quieres evitar movimiento vertical en Y).
            targetPosition.y = transform.position.y; // Opcional: Fija la altura (Y) si no quieres que suba/baje.

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // Mira siempre al personaje (opcional, para perspectiva fija).
            transform.LookAt(target);
        }
    }
}