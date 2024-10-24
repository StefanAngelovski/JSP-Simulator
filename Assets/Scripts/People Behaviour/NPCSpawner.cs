using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCSpawner : MonoBehaviour
{
    public NPCDatabase npcDatabase;
    public int npcCount = 5;
    public Collider spawnArea;
    public float moveInterval = 2f;
    public float moveDistance = 5f;
    public float obstacleDetectionDistance = 1f;
    public float angleThreshold = 30f;
    public float rotationSpeed = 10f;        // Added to control rotation smoothness
    public float movementThreshold = 0.1f;   // Added to detect when NPC is actually moving

    private List<GameObject> spawnedNPCs = new List<GameObject>();

    void Start()
    {
        SpawnNPCs();
    }

    void SpawnNPCs()
    {
        for (int i = 0; i < npcCount; i++)
        {
            GameObject npcPrefab = npcDatabase.npcPrefabs[Random.Range(0, npcDatabase.npcPrefabs.Count)];
            Vector3 randomPosition = GetRandomPositionInCollider();
            GameObject npc = Instantiate(npcPrefab, randomPosition, Quaternion.identity);
            spawnedNPCs.Add(npc);
            
            // Configure NavMeshAgent
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.angularSpeed = rotationSpeed;
                agent.updateRotation = false; // We'll handle rotation manually
            }
            
            StartCoroutine(MoveNPC(npc));
        }
    }

    Vector3 GetRandomPositionInCollider()
    {
        Vector3 randomPosition = Vector3.zero;
        bool positionFound = false;
        int attempts = 0;
        
        while (!positionFound && attempts < 10)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                spawnArea.bounds.center.y,
                Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z)
            );

            // Use NavMesh sampling to ensure valid position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                randomPosition = hit.position;
                positionFound = true;
            }

            attempts++;
        }

        return randomPosition;
    }

    IEnumerator MoveNPC(GameObject npc)
    {
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
        Animator animator = npc.GetComponent<Animator>();
        
        while (true)
        {
            // Generate new destination
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            Vector3 newDestination = npc.transform.position + randomDirection * moveDistance;

            // Sample valid NavMesh position for destination
            NavMeshHit hit;
            if (NavMesh.SamplePosition(newDestination, out hit, moveDistance, NavMesh.AllAreas))
            {
                newDestination = hit.position;
            }

            // Check for obstacles
            if (!DetectObstacle(npc.transform.position, randomDirection))
            {
                agent.SetDestination(newDestination);
            }
            else
            {
                randomDirection = Quaternion.Euler(0, angleThreshold, 0) * randomDirection;
                newDestination = npc.transform.position + randomDirection * moveDistance;
                if (NavMesh.SamplePosition(newDestination, out hit, moveDistance, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }

            // Movement and animation loop
            bool wasMoving = false;
            while (!agent.pathStatus.Equals(NavMeshPathStatus.PathComplete) || 
                   agent.remainingDistance > agent.stoppingDistance)
            {
                // Check if actually moving
                bool isMoving = agent.velocity.magnitude > movementThreshold;
                
                // Handle rotation only when moving
                if (isMoving)
                {
                    // Calculate desired rotation
                    Vector3 direction = agent.velocity.normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        npc.transform.rotation = Quaternion.Slerp(
                            npc.transform.rotation,
                            targetRotation,
                            Time.deltaTime * rotationSpeed
                        );
                    }
                }

                // Update animation state only when movement state changes
                if (isMoving != wasMoving && animator != null)
                {
                    animator.SetBool("IsWalking", isMoving);
                    wasMoving = isMoving;
                }

                yield return null;
            }

            // Ensure animation is stopped when destination is reached
            if (animator != null && wasMoving)
            {
                animator.SetBool("IsWalking", false);
            }

            yield return new WaitForSeconds(moveInterval);
        }
    }

    private bool DetectObstacle(Vector3 origin, Vector3 direction)
    {
        return Physics.Raycast(origin, direction, obstacleDetectionDistance);
    }
}