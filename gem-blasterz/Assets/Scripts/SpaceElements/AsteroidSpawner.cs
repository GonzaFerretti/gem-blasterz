using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AsteroidSpawner : MonoBehaviour
{
    public AsteroidBehaviour asteroid;
    public float asteroidsSpeed, minAsteroidSize, maxAsteroidSize, spawnFrequency, xPosAmplitude, minAsteroidRotSpeed, maxAsteroidRotSpeed, asteroidDeathThreshold;
    public int asteroidPoolInitialCount = 25;
    public List<GameObject> asteroids;

    public ObjectPool<AsteroidBehaviour> asteroidPool;
    public float timer, spawnSide = 1, xPosition;

    void Start()
    {
        asteroidPool = new ObjectPool<AsteroidBehaviour>(
            CreateNew, 
            PrepareAsteroid, 
            instance => instance.gameObject.SetActive(false), defaultCapacity: asteroidPoolInitialCount);
        asteroids = new List<GameObject>();
        SpawnAsteroid();
    }

    private AsteroidBehaviour CreateNew()
    {
        AsteroidBehaviour newAsteroid = Instantiate(asteroid, transform);
        newAsteroid.transform.position = transform.position;
        newAsteroid.OnDestroy += destroyedAsteroid => asteroidPool.Release(destroyedAsteroid);
        return newAsteroid;
    }

    private void PrepareAsteroid(AsteroidBehaviour instance)
    {
        instance.transform.position = transform.position;
        instance.gameObject.SetActive(true);
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
        var instance = asteroidPool.Get();
        instance.Init(Random.Range(minAsteroidRotSpeed, maxAsteroidRotSpeed), 
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
