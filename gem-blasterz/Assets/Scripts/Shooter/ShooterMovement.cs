using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; 
    private Vector2 moveInput; 
    private Rigidbody shooterRB; 

    private void Awake()
    {
        shooterRB = GetComponent<Rigidbody>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(0f, moveInput.y, moveInput.x);
        shooterRB.velocity = movement * moveSpeed;
    }
}
