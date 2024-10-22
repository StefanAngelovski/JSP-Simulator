using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WalkHuman : MonoBehaviour
{
    public float stopDistance = 2f; // Distance to stop and destroy the object
    public float searchRadius = 5f; // Radius within which to move randomly
    public float waitTimeBetweenMovements = 2f; // Time delay between movements
    public float minDistanceBetweenUnits = 5f; // Minimum distance between units

    private NavMeshAgent agent; // Reference to the NavMeshAgent component

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component

        // Start the coroutine for random movements
        StartCoroutine(SetNewTargetPointRoutine()); // Begin the coroutine for random movement
    }

    private void Update()
    {
      

        // Check and adjust positions if too close to other units
        AdjustPositionIfTooClose();
    }

    // Coroutine for picking new movement points with a delay
    private IEnumerator SetNewTargetPointRoutine()
    {
        while (true)
        {
            // Set a new movement point
            SetNewTargetPoint();

            // Wait for a certain amount of time before selecting another position
            yield return new WaitForSeconds(waitTimeBetweenMovements);
        }
    }

    // Function to set a new random point within the search radius
    private void SetNewTargetPoint()
    {
        Vector3 randomPoint;
        NavMeshHit hit;
        int attempts = 0;
        bool validPointFound = false;

        // Try to find a valid point within a certain number of attempts
        while (attempts < 10 && !validPointFound)
        {
            // Get a random point within a sphere around the current position
            randomPoint = transform.position + Random.insideUnitSphere * searchRadius;
            randomPoint.y = transform.position.y; // Keep the original height

            // Check if the random point is valid on the NavMesh
            if (NavMesh.SamplePosition(randomPoint, out hit, searchRadius, NavMesh.AllAreas))
            {
                // Check distance from all other active WalkHuman objects
                if (IsValidDistance(hit.position))
                {
                    agent.SetDestination(hit.position); // Set the agent's destination to the new point
                    validPointFound = true; // Mark that a valid point was found
                }
            }
            attempts++;
        }

        // Optional: if no valid point is found, you can log a warning
        if (!validPointFound)
        {
            Debug.LogWarning($"No valid target point found for {gameObject.name} after 10 attempts.");
        }
    }

    // Function to check if the new position is far enough from other units
    private bool IsValidDistance(Vector3 newPosition)
    {
        // Find all WalkHuman objects in the scene
        WalkHuman[] allHumans = FindObjectsOfType<WalkHuman>();
        foreach (WalkHuman human in allHumans)
        {
            if (human != this) // Skip the current object
            {
                if (Vector3.Distance(newPosition, human.transform.position) < minDistanceBetweenUnits)
                {
                    return false; // Too close to another unit
                }
            }
        }
        return true; // Valid distance
    }

    // Function to adjust position if too close to other units
    private void AdjustPositionIfTooClose()
    {
        // Find all WalkHuman objects in the scene
        WalkHuman[] allHumans = FindObjectsOfType<WalkHuman>();
        foreach (WalkHuman human in allHumans)
        {
            if (human != this) // Skip the current object
            {
                float distance = Vector3.Distance(transform.position, human.transform.position);
                if (distance < minDistanceBetweenUnits)
                {
                    // Move this object away from the other object
                    Vector3 direction = (transform.position - human.transform.position).normalized;
                    transform.position += direction * (minDistanceBetweenUnits - distance); // Move apart
                }
            }
        }
    }

    // Optional: Handle click event to destroy the object
    private void OnMouseDown()
    {
        Destroy(gameObject);
    }
}
