using System.Collections;
using System.Collections.Generic;
using Shooter;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 6f; 
    [SerializeField] private int damage = 2;
    [SerializeField] private Team team;

    private Vector3 direction;

    public void Initialize(Vector3 shootDirection, Team team)
    {
        this.direction = shootDirection.normalized;
        this.team = team;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject collidedObject = other.gameObject;

        if (TryGetReceiver(collidedObject, out var receiver) && receiver.CanDamage(team))
        {
            receiver.ReceiveDamage(damage);
            Destroy(gameObject);
        }
    }

    private static bool TryGetReceiver(GameObject collidedObject, out IDamageReceiver receiver)
    {
        return collidedObject.TryGetComponent<IDamageReceiver>(out receiver) || (collidedObject.transform.parent != null && TryGetReceiver(collidedObject.transform.parent.gameObject, out receiver));
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
