using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float RunSpeed = 7;
    public float RotationSpeed = 250;
    public Animator animator;

    private float x, y;
    private bool canMove = true; // Controlador de movimiento

    void Update()
    {
        if (!canMove) return; // Si no puede moverse, salir del Update

        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");

        transform.Rotate(0, x * Time.deltaTime * RotationSpeed, 0);
        transform.Translate(0, 0, y * Time.deltaTime * RunSpeed);

        animator.SetFloat("VelX", x);
        animator.SetFloat("VelY", y);
    }

    // Método público para controlar el movimiento
    public void EnableMovement(bool enable)
    {
        canMove = enable;

        // Resetear animaciones si se desactiva el movimiento
        if (!enable)
        {
            animator.SetFloat("VelX", 0);
            animator.SetFloat("VelY", 0);
        }
    }
}