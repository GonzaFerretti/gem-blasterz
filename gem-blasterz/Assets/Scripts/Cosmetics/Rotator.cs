using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("The axis around which the object will rotate.")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("The maximum angle of rotation in degrees.")]
    public float amplitude = 45f;

    [Tooltip("The speed of the rotation oscillation.")]
    public float speed = 1f;

    private float _time;

    public bool idleAnimation;

    public float fadeTime, fadeSpeed;
    public float currentTimeMultiplier;

    void Update()
    {
        CycleAnimation();
        Sway();
    }

    public void Sway()
    {
        // Increment time based on speed
        _time += Time.deltaTime * speed;

        // Calculate the new rotation angle using sine wave
        float angle = Mathf.Sin(_time) * amplitude * currentTimeMultiplier;

        // Apply the rotation to the object
        transform.localRotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);
    }

    public void CycleAnimation()
    {
        fadeTime += Time.deltaTime;
        currentTimeMultiplier = (Mathf.Cos(2 * Mathf.PI * fadeTime / fadeSpeed + Mathf.PI) + 1) / 2;



    }
}