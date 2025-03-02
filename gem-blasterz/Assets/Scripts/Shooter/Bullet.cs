using System.Collections;
using System.Collections.Generic;
using Shooter;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 6f; 
    [SerializeField] private int damage = 2;
    [SerializeField] private GameObject shooter;

    private Vector3 direction;

    public void Initialize(Vector3 shootDirection, GameObject shooter)
    {
        this.direction = shootDirection.normalized;
        this.shooter = shooter;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject collidedObject = other.gameObject;
        if (other.gameObject == shooter)
        {
            return;
        };

        var properHit = false;
        if (collidedObject.TryGetComponent<ShooterController>(out var otherShooter))
        {
            otherShooter.ReceiveDamage(damage);
            properHit = true;
        }

        if (collidedObject.TryGetComponent<Ship>(out var hitShip))
        {
            hitShip.ReceiveDamage();
            properHit = true;
        }
        
        if (properHit)
            Destroy(gameObject);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
