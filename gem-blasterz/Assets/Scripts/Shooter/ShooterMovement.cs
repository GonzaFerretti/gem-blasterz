using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShooterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float aimSpeed = 5f;
    private Vector2 moveInput;
    private Vector2 markerInput;
    private Rigidbody shooterRB;
    [SerializeField] private Transform markerTransform;
    private float maxHP = 10f;
    private float hp = 10f;
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 1f;
    private bool isDisabled = false;

    [SerializeField] private PlayerInput playerInput; 

    private float lastFireTime; 

    public void decreaseHP(float damage){
        hp = Mathf.Max(hp - damage,0);
        if (hp <= 0)
        {
            StartCoroutine(HandleDeath());
        }
    }

    public void InitializeInput()
    {
        playerInput.enabled = true;
    }

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
        if (!GeneralManager.GameStarted) return;
        
        if (isDisabled)
            return;
        healthBar.value = hp / maxHP;
        Vector3 movement = new Vector3(0f, moveInput.y, moveInput.x);
        shooterRB.velocity = movement * moveSpeed;
        Vector3 markerMovement = new Vector3(0f, markerInput.y, markerInput.x);
        markerTransform.position += markerMovement * aimSpeed * Time.deltaTime;
    }
    public void OnAim(InputAction.CallbackContext context)
    {
        markerInput = context.ReadValue<Vector2>();
    }

    public void Shoot(){
        if (Time.time >= lastFireTime + fireCooldown && !isDisabled){
            Debug.Log("Shooting");
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
}
