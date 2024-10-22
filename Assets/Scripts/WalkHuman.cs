using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WalkHuman : MonoBehaviour
{
    public float moveRadius = 10f; // The radius within which the objects can move
    public float changeDirectionTime = 3f; // Time interval to change direction
    public float waitTime = 2f; // Time the humans will wait before moving again
    public Transform busStop; // Assign the bus stop transform in the inspector

    private float timer;
    private bool isWaiting; // To track if the human is currently waiting

    void Start()
    {
        timer = changeDirectionTime;
        isWaiting = false; // Start as not waiting
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // If the timer has expired, pick a new action for each human
        if (timer <= 0f)
        {
            GameObject[] humans = GameObject.FindGameObjectsWithTag("Human"); // Find all GameObjects with the tag "Human"

            foreach (GameObject human in humans)
            {
                MoveHuman(human);
            }
            timer = changeDirectionTime;
        }
    }

    void MoveHuman(GameObject human)
    {
        NavMeshAgent navMeshAgent = human.GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component

        if (navMeshAgent != null)
        {
            if (isWaiting)
            {
                // If waiting, do nothing for a while
                StartCoroutine(WaitAtBusStop(human, waitTime));
            }
            else
            {
                // Randomly decide to move or wait (lower chance to wait)
                if (Random.Range(0f, 1f) < 0.2f) // 20% chance to wait
                {
                    isWaiting = true; // Set waiting state
                }
                else
                {
                    MoveToRandomPosition(human, navMeshAgent); // Move to a random position
                }
            }
        }
    }

    void MoveToRandomPosition(GameObject human, NavMeshAgent navMeshAgent)
    {
        Vector3 randomDirection = Random.insideUnitSphere * moveRadius; // Get a random direction within the radius
        randomDirection += human.transform.position; // Offset it by the human's current position

        NavMeshHit hit; // Variable to store the hit information

        // Check if the random point is on the NavMesh and get the closest point on the NavMesh
        if (NavMesh.SamplePosition(randomDirection, out hit, moveRadius, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position); // Move the agent to the new position
        }
    }

    IEnumerator WaitAtBusStop(GameObject human, float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration); // Wait for the specified duration

        NavMeshAgent navMeshAgent = human.GetComponent<NavMeshAgent>();
        if (navMeshAgent != null && busStop != null)
        {
            // After waiting, randomly decide to move towards the bus stop
            if (Random.Range(0f, 1f) < 0.5f) // 50% chance to move towards the bus stop
            {
                navMeshAgent.SetDestination(busStop.position);
            }
        }

        isWaiting = false; // Reset waiting state
    }
}
