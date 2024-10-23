using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHuman : MonoBehaviour
{
    public GameObject[] prefabs; // Array of prefabs to spawn
    public Transform spawnPlatform; // Single spawn platform
    public float minSpawnInterval = 1f; // Minimum spawn interval
    public float maxSpawnInterval = 5f; // Maximum spawn interval
    public int maxObjects = 20; // Maximum number of objects that can be spawned at once
    public float minDistanceBetweenUnits = 3f; // Minimum distance between spawned units
    public int initialSpawnCount = 10; // Number of objects to spawn initially

    private List<Vector3> spawnPositions = new List<Vector3>(); // List to track spawn positions

    private void Start()
    {
        // Spawn an initial set of objects
        InitialPopulation();

        // Start the coroutine for spawning objects at random intervals
        StartCoroutine(SpawnObjects());
    }

    void InitialPopulation()
    {
        // Spawn an initial set of objects to avoid a slow buildup
        for (int i = 0; i < initialSpawnCount; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            if (spawnPosition != Vector3.zero) // Ensure a valid position is found
            {
                SpawnPrefab(spawnPosition);
            }
        }
    }

    private IEnumerator SpawnObjects()
    {
        while (true)
        {
            // Wait for a random time between the specified intervals
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));

            // Randomly determine how many objects to spawn (between 1 and 5)
            int spawnCount = Random.Range(1, 6); // 1 to 5 inclusive

            // Check how many objects can be spawned without exceeding the limit
            int currentObjectCount = spawnPositions.Count;
            int objectsToSpawn = Mathf.Min(spawnCount, maxObjects - currentObjectCount);

            // Spawn the determined number of prefabs
            for (int i = 0; i < objectsToSpawn; i++)
            {
                Vector3 spawnPosition = GetValidSpawnPosition();
                if (spawnPosition != Vector3.zero) // Ensure a valid position is found
                {
                    SpawnPrefab(spawnPosition);
                }
            }
        }
    }

    private Vector3 GetValidSpawnPosition()
    {
        Vector3 randomPoint;
        int attempts = 0;

        do
        {
            // Get a random point within the bounds of the spawn platform
            randomPoint = GetRandomPointInPlatform(spawnPlatform);
            attempts++;

        } while (!IsValidSpawnPosition(randomPoint) && attempts < 10); // Limit attempts to avoid infinite loop

        return attempts < 10 ? randomPoint : Vector3.zero; // Return zero if a valid position wasn't found
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        foreach (Vector3 existingPosition in spawnPositions)
        {
            if (Vector3.Distance(existingPosition, position) < minDistanceBetweenUnits)
            {
                return false; // Too close to an existing unit
            }
        }

        // Check against existing humans in the scene
        GameObject[] existingHumans = GameObject.FindGameObjectsWithTag("Human");
        foreach (GameObject human in existingHumans)
        {
            if (Vector3.Distance(human.transform.position, position) < minDistanceBetweenUnits)
            {
                return false; // Too close to an existing human
            }
        }

        // If valid, add the position to the list
        spawnPositions.Add(position);
        return true;
    }

    private void SpawnPrefab(Vector3 position)
    {
        // Choose a random prefab
        GameObject randomPrefab = prefabs[Random.Range(0, prefabs.Length)];

        // Instantiate the random prefab at the specified position
        GameObject spawnedObject = Instantiate(randomPrefab, position, Quaternion.identity);

        // Add movement script to the spawned object
        WalkHuman movement = spawnedObject.AddComponent<WalkHuman>();
        // Additional setup for the movement can go here
    }

    private Vector3 GetRandomPointInPlatform(Transform platform)
    {
        Collider platformCollider = platform.GetComponent<Collider>();
        if (platformCollider != null)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(platformCollider.bounds.min.x, platformCollider.bounds.max.x),
                platform.position.y, // Keep the y position the same as the platform
                Random.Range(platformCollider.bounds.min.z, platformCollider.bounds.max.z)
            );
            return randomPoint;
        }
        else
        {
            Debug.LogWarning("No Collider found on platform: " + platform.name);
            return platform.position; // Fallback to platform position if no collider
        }
    }
}
