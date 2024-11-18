using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TestSound : MonoBehaviour
{
    AudioSource m_AudioSource;

    public float transpose = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        PlaySound();
    }
    void PlaySound()
    {
        float note = -1;

        if (Input.GetKey(KeyCode.A)) note = 0;  // C
        if (Input.GetKey(KeyCode.S)) note = 2;  // D
        if (Input.GetKey(KeyCode.D)) note = 4;  // E
        if (Input.GetKey(KeyCode.F)) note = 5;  // F
        if (Input.GetKey(KeyCode.G)) note = 7;  // G
        if (Input.GetKey(KeyCode.H)) note = 9;  // A
        if (Input.GetKey(KeyCode.J)) note = 11; // B
        if (Input.GetKey(KeyCode.K)) note = 12; // C
        if (Input.GetKey(KeyCode.L)) note = 14; // D

        if (note >= 0)
        {
            m_AudioSource.pitch = Mathf.Pow(2, (note + transpose) / 12.0f);
            m_AudioSource.Play();
        }
    }
}
