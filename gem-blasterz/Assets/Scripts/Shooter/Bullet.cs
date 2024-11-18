using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 6f; 
    [SerializeField] private int damage = 2;
    [SerializeField] private ShooterController enemy;

    private Vector3 direction;

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized; 
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject collidedObject = other.gameObject;
        if (collidedObject.CompareTag("Player"))
        {
            collidedObject.GetComponent<ShooterController>().decreaseHP(damage);
        }
        Debug.Log($"Hit player: {collidedObject.name}, applying {damage} damage.");
        Destroy(gameObject);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
