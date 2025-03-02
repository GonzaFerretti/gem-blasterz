using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject asteroid;
    public float asteroidsSpeed, minAsteroidSize, maxAsteroidSize, spawnFrequency, xPosAmplitude, minAsteroidRotSpeed, maxAsteroidRotSpeed, asteroidDeathThreshold;
    public List<GameObject> asteroids;
    public float timer, spawnSide = 1, xPosition;

    void Start()
    {
        asteroids = new List<GameObject>();
        SpawnAsteroid();
    }

    void Update()
    {
        if (timer > spawnFrequency)
        {
            SpawnAsteroid();
            timer = 0;
        }
        timer += Time.deltaTime;
    }

    void SpawnAsteroid()
    {
        xPosition = Random.Range(0, Mathf.Abs(xPosAmplitude));
        spawnSide = spawnSide * -1;
        GameObject newAsteroid = Instantiate(asteroid);
        newAsteroid.transform.position = transform.position;
        newAsteroid.transform.parent = transform;
        newAsteroid.GetComponent<AsteroidBehaviour>().Init(Random.Range(minAsteroidRotSpeed, maxAsteroidRotSpeed), 
            asteroidsSpeed, 
            xPosition * spawnSide, 
            Random.Range(minAsteroidSize, maxAsteroidSize),
            asteroidDeathThreshold);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector3(132, asteroidDeathThreshold, 232), Vector3.right * 200);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(new Vector3(transform.position.x + xPosAmplitude, -60, 232), Vector3.up * (asteroidDeathThreshold - transform.position.y));
        Gizmos.DrawRay(new Vector3(transform.position.x - xPosAmplitude, -60, 232), Vector3.up * (asteroidDeathThreshold - transform.position.y));
    }
}
