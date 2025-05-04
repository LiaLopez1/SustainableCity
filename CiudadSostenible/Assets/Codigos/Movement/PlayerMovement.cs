using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    private float originalStepOffset;

    private Animator animator;


    private CharacterController characterController;
  

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;
       
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float VerticallInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, VerticallInput);
        float magnitud = Mathf.Clamp01(movementDirection.magnitude) * speed;
        movementDirection.Normalize();

        //characterController.SimpleMove(movementDirection * magnitud);

        transform.Translate(movementDirection * magnitud * Time.deltaTime, Space.World);

        if(movementDirection != Vector3.zero)
        {
            animator.SetBool("IsMoving", true);

            //transform.forward = movementDirection;
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

    }
}
