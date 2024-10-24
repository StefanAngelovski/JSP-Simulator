using UnityEngine;
using UnityEngine.AI;

public class FlockManager : MonoBehaviour
{
    public static FlockManager FM;
    public GameObject fishPrefab;
    public int numFish = 20;
    public GameObject[] allFish;
    public GameObject spawnPlatform; // Platform on which objects spawn
    public Vector3 goalPos = Vector3.zero;

    [Header("Fish Settings")]
    [Range(0.0f, 5.0f)]
    public float minSpeed;
    [Range(0.0f, 5.0f)]
    public float maxSpeed;
    [Range(1.0f, 10.0f)]
    public float neighbourDistance;
    [Range(1.0f, 5.0f)]
    public float rotationSpeed;

    void Start()
    {
        FM = this;
        allFish = new GameObject[numFish];
        for (int i = 0; i < numFish; i++)
        {
            Vector3 pos = GetRandomPositionOnPlatform();
            allFish[i] = Instantiate(fishPrefab, pos, Quaternion.identity);
        }
        goalPos = this.transform.position;
    }

    void Update()
    {
        if (Random.Range(0, 100) < 10)
        {
            goalPos = GetRandomPositionOnPlatform();
        }
    }

    public Vector3 GetRandomPositionOnPlatform()
    {
        Collider platformCollider = spawnPlatform.GetComponent<Collider>();
        if (platformCollider == null)
        {
            Debug.LogError("Spawn platform does not have a Collider component");
            return Vector3.zero;
        }

        Bounds bounds = platformCollider.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
