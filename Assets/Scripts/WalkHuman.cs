using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkHuman : MonoBehaviour
{
    private NavMeshAgent agent;
    public float moveSpeed = 3.5f; // Speed of the agent
    public float rotationSpeed = 5f; // Speed of rotation to face the target
    public float minWanderTime = 2f; // Minimum time to wander
    public float maxWanderTime = 5f; // Maximum time to wander
    public float wanderRadius = 5f; // Radius within which to wander
    public float minDistanceToOtherHumans = 3f; // Minimum distance to maintain from other humans
    public List<GameObject> allHumans; // List of all human GameObjects

    private Vector3 targetPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        allHumans = new List<GameObject>(GameObject.FindGameObjectsWithTag("Human"));

        StartCoroutine(Wander());
    }

    IEnumerator Wander()
    {
        while (true)
        {
            SetNewTarget();
            yield return new WaitForSeconds(Random.Range(minWanderTime, maxWanderTime));
        }
    }

  void SetNewTarget()
{
    Vector3 newTargetPosition = Vector3.zero; // Initialize the variable
    int attempts = 0; // Add a counter for attempts
    const int maxAttempts = 20; // Set a max attempt limit

    do
    {
        // Generate a random point within a defined radius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        // Find a valid point on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            newTargetPosition = hit.position; // Assign the value if valid
        }
        else
        {
            continue; // If no valid point found, try again
        }

        attempts++; // Increment the attempts count

    } while (!IsPositionValid(newTargetPosition) && attempts < maxAttempts); // Exit if max attempts reached

    // If max attempts reached without a valid position, log a warning
    if (attempts >= maxAttempts)
    {
        Debug.LogWarning("Failed to find a valid target position after multiple attempts.");
        return; // Exit the method if no valid position found
    }

    targetPosition = newTargetPosition;

    // Set the destination for the NavMeshAgent
    agent.SetDestination(targetPosition);
}

    bool IsPositionValid(Vector3 position)
    {
        // Check distance to all other humans
        foreach (GameObject human in allHumans)
        {
            if (human != gameObject) // Don't check against itself
            {
                float distance = Vector3.Distance(position, human.transform.position);
                if (distance < minDistanceToOtherHumans) // If too close, return false
                {
                    return false;
                }
            }
        }

        return true; // Position is valid if all checks passed
    }

    void Update()
    {
        // Smooth rotation to face the target
        Vector3 direction = targetPosition - transform.position;

        // Check if the direction is approximately zero
        if (direction.sqrMagnitude > 0.0001f) // Use squared magnitude for efficiency
        {
            // Ensure that the direction is not zero before looking
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            Debug.LogWarning("Target position is too close to the current position.");
        }
    }
}
