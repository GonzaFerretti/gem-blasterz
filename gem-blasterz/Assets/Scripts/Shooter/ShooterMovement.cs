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
    
    [Header("Read-only")]
    [SerializeField, ReadOnly] private Vector2 moveInput;
    [SerializeField, ReadOnly] private Vector3 lastValidAimInput;
    [SerializeField, ReadOnly] private bool wantsFire;
    [SerializeField, ReadOnly] private float speed;
    private Vector2 markerInput;
    private float maxHP = 10f;
    private float hp = 10f;
    
    [Header("References")]
    [SerializeField] private Transform markerTransform;
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    private Rigidbody shooterRB;
    private bool isDisabled = false;

    [SerializeField] private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction aimAction;
    private InputAction fireAction;
    

    private float lastFireTime;

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
        speed = shooterRB.velocity.magnitude;
    }

    public void ProcessFire(){
        if (Time.time >= lastFireTime + fireCooldown && !isDisabled){
            GameObject bullet = Instantiate(bulletPrefab,firePoint.position,Quaternion.identity);
            Vector3 shootDirection = (markerTransform.position - firePoint.position).normalized;
            bullet.GetComponent<Bullet>().Initialize(shootDirection,this.gameObject);
            lastFireTime = Time.time;
        }
    }

    private IEnumerator HandleDeath()
    {
        Debug.Log("Player is disabled!");
        isDisabled = true; 
        Vector3 movement = new Vector3(0f,0f,0f);
        shooterRB.velocity = movement * moveSpeed;

        yield return new WaitForSeconds(5f); 

        Debug.Log("Player recovered!");
        hp = maxHP;
        isDisabled = false;
    }

    public bool CanDamage(Team team)
    {
        return team != this.team;
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
