using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float aimSpeed = 5f;
    private Vector2 moveInput;
    private Vector2 markerInput;
    private Rigidbody shooterRB;
    [SerializeField] private Transform markerTransform;

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
        Vector3 markerMovement = new Vector3(0f, markerInput.y, markerInput.x);
        markerTransform.position += markerMovement * aimSpeed * Time.deltaTime;
    }
    public void OnAim(InputAction.CallbackContext context)
    {
        // Get the right stick movement input as a Vector2
        markerInput = context.ReadValue<Vector2>();
    }
}
