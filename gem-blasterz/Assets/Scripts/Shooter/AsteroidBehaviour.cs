using System;
using System.Collections;
using System.Collections.Generic;
using Shooter;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class AsteroidBehaviour : MonoBehaviour, IDamageReceiver
{
    public float deathThreshold, rotSpeed, velocity;
    public Action<AsteroidBehaviour> OnDestroy; 
    Rigidbody rb;
    
    [SerializeField]
    private TrailRenderer trail;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //transform.localRotation = Quaternion.AngleAxis(Time.deltaTime * rotSpeed, transform.up);
        //transform.Rotate(transform.up, rotSpeed);
        if (transform.position.y > deathThreshold)
        {
            Destroy();
        }

        transform.position += Vector3.up * Time.deltaTime * velocity;
        transform.Rotate(transform.up, Time.deltaTime * rotSpeed);
    }

    public void Init(float _rotSpeed, float vel, float posX, float scale, float death)
    {
        trail.Clear();
        transform.rotation = Random.rotation;
        transform.localScale = new Vector3(scale, scale, scale);
        transform.position = new Vector3(transform.position.x + posX, transform.position.y, transform.position.z);
        deathThreshold = death;
        velocity = vel;
        rotSpeed = _rotSpeed;
    }

    public bool CanDamage(Team team)
    {
        return true;
    }

    public void ReceiveDamage(float damage) => Destroy();

    private void Destroy()
    {
        OnDestroy?.Invoke(this);
    }
}
