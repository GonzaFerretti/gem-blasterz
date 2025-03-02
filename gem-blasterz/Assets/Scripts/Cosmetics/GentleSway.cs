using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GentleSway : MonoBehaviour
{
    [Header("Position Sway Settings")]
    public Vector3 swayDirection = new Vector3(1, 0, 0); // Direction of the positional sway
    public float swayAmplitude = 1.0f; // Amplitude of positional sway
    public float swaySpeed = 1.0f; // Speed of positional sway
    public float swayTimeOffset = 0;

    [Header("Rotation Sway Settings")]
    public Vector3 rotationAxis = new Vector3(0, 0, 1); // Axis of rotational sway
    public float rotationAmplitude = 10.0f; // Amplitude of rotational sway in degrees
    public float rotationSpeed = 1.0f; // Speed of rotational sway
    public float rotationTimeOffset = 0;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // Record the initial position and rotation of the object
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Calculate positional sway
        float positionOffset = Mathf.Sin((Time.time + swayTimeOffset) * swaySpeed) * swayAmplitude;
        Vector3 swayOffset = swayDirection.normalized * positionOffset;

        // Calculate rotational sway
        float rotationOffset = Mathf.Sin((Time.time + rotationTimeOffset) * rotationSpeed) * rotationAmplitude;
        Quaternion swayRotation = Quaternion.Euler(rotationAxis.normalized * rotationOffset);

        // Apply position and rotation
        transform.position = initialPosition + swayOffset;
        transform.rotation = initialRotation * swayRotation;
    }
}
