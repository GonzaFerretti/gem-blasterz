using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Rotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("The axis around which the object will rotate.")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("The maximum angle of rotation in degrees.")]
    public float amplitude = 45f;

    [Tooltip("The speed of the rotation oscillation.")]
    public float speed = 1f;

    [Header("Cycle Animation Settings")]
    [Tooltip("The interval between each cycle animation in seconds.")]
    public float cycleInterval = 5f;
    public float cycleIntervalVariation = 1;

    public float fadeSpeed;
    private float _time;
    private float _cycleTimer;
    private bool _isCycling;

    [Range(0, 1)]
    public float currentAmplitudeMultiplier;

    private void Awake()
    {
        cycleInterval = cycleInterval + Random.Range(-cycleIntervalVariation / 2, cycleIntervalVariation / 2);
    }

    void Update()
    {
        Sway();
        HandleCycleAnimation();
    }

    public void Sway()
    {
        // Increment time based on speed
        _time += Time.deltaTime * speed;

        // Calculate the new rotation angle using sine wave
        float angle = Mathf.Sin(_time) * amplitude * currentAmplitudeMultiplier;

        // Apply the rotation to the object
        transform.localRotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);
    }

    private void HandleCycleAnimation()
    {
        // Increment the cycle timer
        _cycleTimer += Time.deltaTime;

        if (!_isCycling && _cycleTimer >= cycleInterval)
        {
            rotationAxis = Random.insideUnitSphere;
            // Start the cycle animation
            _cycleTimer = 0f;
            _isCycling = true;
        }

        if (_isCycling)
        {
            // Perform the fade effect
            currentAmplitudeMultiplier = (Mathf.Cos(2 * Mathf.PI * _cycleTimer / fadeSpeed + Mathf.PI) + 1) / 2 - 0.5f;

            // End the cycle animation after one full fade cycle
            if (_cycleTimer >= 0.75 * fadeSpeed)
            {
                _isCycling = false;
                _cycleTimer = 0f;
            }
        }
    }
}