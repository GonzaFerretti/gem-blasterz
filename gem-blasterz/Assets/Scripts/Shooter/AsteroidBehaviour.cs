using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AsteroidBehaviour : MonoBehaviour
{
    public float deathThreshold, rotSpeed;
    Rigidbody rb;
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.localRotation = Quaternion.AngleAxis(Time.deltaTime * rotSpeed, transform.up);
        //transform.Rotate(transform.up, rotSpeed);
        if (transform.position.y > deathThreshold)
        {
            Destroy(this.gameObject);
        }
    }

    public void Init(float rotSpeed, float vel, float posX, float scale, float death)
    {
        transform.rotation = Random.rotation;
        transform.localScale = new Vector3(scale, scale, scale);
        transform.position = new Vector3(transform.position.x + posX, transform.position.y, transform.position.z);
        GetComponent<Rigidbody>().velocity = new Vector3(0, vel, 0);
        deathThreshold = death;
    }
}
