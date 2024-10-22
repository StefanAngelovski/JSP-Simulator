using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHuman : MonoBehaviour
{
    // List of human prefabs
    public List<GameObject> humanPrefabs;

    // Surface area for spawning
    public GameObject surfaceArea;

    // Number of initial humans to spawn
    public int initialSpawnCount = 5;

    // Time interval between spawning new humans
    public float spawnInterval = 1.0f;

    private Vector3 surfaceMin;
    private Vector3 surfaceMax;

    void Start()
    {
        // Calculate the boundaries of the surface area
        CalculateSurfaceBounds();

        // Spawn initial humans
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnRandomHuman();
        }

        // Start spawning new humans every second
        StartCoroutine(SpawnHumanOverTime());
    }

    void CalculateSurfaceBounds()
    {
        // Assuming surfaceArea has a BoxCollider or MeshRenderer that defines the area
        Renderer surfaceRenderer = surfaceArea.GetComponent<Renderer>();
        surfaceMin = surfaceRenderer.bounds.min;
        surfaceMax = surfaceRenderer.bounds.max;
    }

    void SpawnRandomHuman()
    {
        // Select a random human prefab from the list
        int randomIndex = Random.Range(0, humanPrefabs.Count);
        GameObject humanToSpawn = humanPrefabs[randomIndex];

        // Generate a random position within the surface area bounds
        Vector3 randomPosition = new Vector3(
            Random.Range(surfaceMin.x, surfaceMax.x),
            surfaceMin.y, // Spawn at surface level
            Random.Range(surfaceMin.z, surfaceMax.z)
        );

        // Instantiate the human prefab at the random position
        Instantiate(humanToSpawn, randomPosition, Quaternion.identity);
    }

    IEnumerator SpawnHumanOverTime()
    {
        while (true)
        {
            // Wait for the specified interval
            yield return new WaitForSeconds(spawnInterval);

            // Spawn a random human
            SpawnRandomHuman();
        }
    }
}
