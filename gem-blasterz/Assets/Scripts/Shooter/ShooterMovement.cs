using System;
using System.Collections;
using System.Collections.Generic;
using Shooter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShooterController : MonoBehaviour, IDamageReceiver
{
    
    [Header("Config Variables")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float deceleration = 0.1f;
    [SerializeField] private float accelSpeed = 5f;
    [SerializeField] private float aimSpeed = 5f;
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private float fireIndicatorDistance = 20f;
    [SerializeField] private Team team;
    [SerializeField] private Bounds movementBounds;
    [SerializeField] private float boundsCheckRadius;
    [SerializeField] private Vector3 boundsCheckCenterOffset;
    
    [Header("Read-only")]
    [SerializeField, ReadOnly] private Vector2 moveInput;
    [SerializeField, ReadOnly] private Vector3 lastValidAimInput;
    [SerializeField, ReadOnly] private bool wantsFire;
    [SerializeField, ReadOnly] private float speed;
    [SerializeField, ReadOnly] private Vector3 velocity;
    private Vector2 markerInput;
    private float maxHP = 10f;
    private float hp = 10f;
    
    [Header("References")]
    [SerializeField] private Transform markerTransform;
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator animator;
    private Rigidbody shooterRB;
    private bool isDisabled = false;

    [SerializeField] private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction aimAction;
    private InputAction fireAction;
    

    private float lastFireTime;
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    private static readonly int DeadFlag = Animator.StringToHash("IsDead");
    private static readonly int MoveXValue = Animator.StringToHash("MovX");
    private static readonly int MoveYValue = Animator.StringToHash("MovY");

    public void InitializeInput()
    {
        playerInput.enabled = true;
        moveAction = playerInput.actions.FindAction("MoveShooter");
        aimAction = playerInput.actions.FindAction("Aim");
        fireAction = playerInput.actions.FindAction("Fire");
    }

    private void Awake()
    {
        shooterRB = GetComponent<Rigidbody>();
    }

    private void ReadInputs()
    {
        // Move
        moveInput = moveAction.ReadValue<Vector2>();
        moveInput = moveInput.normalized * Math.Min(1, moveInput.magnitude);
        
        // Aim
        
        if (playerInput.currentControlScheme == "Keyboard")
        {
            Vector3 mousePos = Input.mousePosition;
            markerInput = (mousePos - Camera.main.WorldToScreenPoint(firePoint.position)).normalized;
        }
        else
        {
            markerInput = aimAction.ReadValue<Vector2>();
            markerInput = markerInput.normalized * Math.Min(1, markerInput.magnitude);
        }
        
        // Fire
        wantsFire = fireAction.ReadValue<float>() > 0;
    }

    private void FixedUpdate()
    {
        if (!GeneralManager.GameStarted) return;
        
        if (isDisabled)
            return;
        
        ReadInputs();
        
        healthBar.value = hp / maxHP;
        ProcessMovement();
        ProcessAim();
        
        if (wantsFire)
            ProcessFire();
    }

    private void ProcessAim()
    {
        Vector3 markerMovement = new Vector3(markerInput.x, markerInput.y, 0f);
        if (markerMovement.magnitude != 0)
        {
            lastValidAimInput = markerMovement.normalized;
        }
        markerTransform.position = firePoint.position + lastValidAimInput * fireIndicatorDistance;

    }

    private void ProcessMovement()
    {
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0);
        shooterRB.velocity += movement * accelSpeed;
        var maxLength = shooterRB.velocity.magnitude;

        var inputSize = movement.magnitude;
        if (inputSize == 0)
        {
            shooterRB.velocity -= shooterRB.velocity.normalized * (Math.Min(maxLength, deceleration * Time.deltaTime));
        }

        var max = inputSize > 0 ?  moveSpeed * inputSize : moveSpeed;
        shooterRB.velocity = shooterRB.velocity.normalized * Math.Clamp(shooterRB.velocity.magnitude, 0, max);
        
        KeepPositionInsideBounds();
        velocity = shooterRB.velocity;
        
        
        animator.SetFloat(MoveXValue, Mathf.InverseLerp(0, speed, Math.Abs(shooterRB.velocity.x)) * Mathf.Sign(shooterRB.velocity.x));
        animator.SetFloat(MoveYValue, Mathf.InverseLerp(0, speed, Math.Abs(shooterRB.velocity.y)) * Mathf.Sign(shooterRB.velocity.y));
        speed = shooterRB.velocity.magnitude;
    }
    
    void KeepPositionInsideBounds()
    {
        Vector3 position = transform.position + boundsCheckCenterOffset; // Apply offset
        Vector3 velocity = shooterRB.velocity;

        // Calculate the min and max allowed positions within the bounds, considering the radius
        Vector3 min = movementBounds.min + new Vector3(boundsCheckRadius, boundsCheckRadius, 0);
        Vector3 max = movementBounds.max - new Vector3(boundsCheckRadius, boundsCheckRadius, 0);

        // Check X axis
        if (position.x < min.x)
        {
            position.x = min.x;
            velocity.x = Mathf.Min(0, velocity.x); // Stop or reverse velocity
        }
        else if (position.x > max.x)
        {
            position.x = max.x;
            velocity.x = Mathf.Max(0, velocity.x);
        }

        // Check Y axis
        if (position.y < min.y)
        {
            position.y = min.y;
            velocity.y = Mathf.Min(0, velocity.y);
        }
        else if (position.y > max.y)
        {
            position.y = max.y;
            velocity.y = Mathf.Max(0, velocity.y);
        }

        // Apply the clamped position, adjusting for the offset
        transform.position = position - boundsCheckCenterOffset;

        // Apply the modified velocity
        shooterRB.velocity = velocity;
    }

    [ExecuteInEditMode]
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(movementBounds.center, movementBounds.size);
        Gizmos.DrawWireSphere(transform.position + boundsCheckCenterOffset, boundsCheckRadius);
    }

    public void ProcessFire(){
        if (Time.time >= lastFireTime + fireCooldown && !isDisabled){
            GameObject bullet = Instantiate(bulletPrefab,firePoint.position,Quaternion.identity);
            Vector3 shootDirection = (markerTransform.position - firePoint.position).normalized;
            bullet.GetComponent<Bullet>().Initialize(shootDirection, team);
            lastFireTime = Time.time;
            animator.SetTrigger(ShootTrigger);
        }
    }

    private IEnumerator HandleDeath()
    {
        Debug.Log("Player is disabled!");
        isDisabled = true; 
        Vector3 movement = new Vector3(0f,0f,0f);
        shooterRB.velocity = movement * moveSpeed;
        animator.SetBool(DeadFlag, true);

        yield return new WaitForSeconds(5f); 

        Debug.Log("Player recovered!");
        hp = maxHP;
        isDisabled = false;
        animator.SetBool(DeadFlag, false);
    }

    public bool CanDamage(Team team)
    {
        return !isDisabled && team != this.team;
    }

    public void ReceiveDamage(float damage)
    {
        hp = Mathf.Max(hp - damage,0);
        if (hp <= 0)
        {
            StartCoroutine(HandleDeath());
        }
    }
}
