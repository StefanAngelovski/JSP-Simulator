using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkHuman : MonoBehaviour
{
    public float death_circle_radius = 50f; // Distance from the game_area center before humans are destroyed
    public float idleDuration = 3f; // Duration for which the human will stay idle
    public float idleProbability = 0.2f; // Probability of entering idle state (20%)
    public float neighborRadius = 3f; // Radius to find nearby humans
    public float separationDistance = 7f; // Minimum distance to maintain from other humans
    public float minimumDistance = 3f; // Minimum distance to maintain from other humans
    public float alignmentWeight = 0.1f; // Weight for alignment behavior
    public float separationWeight = 3f; // Weight for separation behavior
    public float randomMoveInterval = 2f; // Time interval to choose a new random destination
    public float collisionAvoidanceDistance = 1f; // Distance to check for collisions
    public LayerMask obstacleLayer; // Layer to check for obstacles

    private GameObject game_area; // To hold the object with the "WalkableArea" tag
    private List<GameObject> allHumans; // List to hold all objects tagged as "Human"
    private NavMeshAgent agent; // NavMeshAgent component of the human

    private bool isIdle; // To check if the human is currently idle
    private float idleTimer; // Timer to keep track of idle duration
    private float moveTimer; // Timer to keep track of when to set a new random destination

    void Start()
    {
        // Find the object with the "WalkableArea" tag
        game_area = GameObject.FindWithTag("WalkableArea");

        if (game_area == null)
        {
            Debug.LogError("No object with tag 'WalkableArea' found in the scene.");
            return;
        }

        // Find all objects with the "Human" tag and store them in the list
        allHumans = new List<GameObject>(GameObject.FindGameObjectsWithTag("Human"));
        isIdle = false; // Initially not idle

        agent = GetComponent<NavMeshAgent>();
        // Set a random initial destination
        SetRandomDestination();
    }

    void Update()
    {
        if (isIdle)
        {
            UpdateIdleState();
        }
        else
        {
            MoveHuman();
        }
    }

    void MoveHuman()
    {
        // Update the move timer
        moveTimer += Time.deltaTime;

        // Check for obstacles in front of the agent
        if (Physics.Raycast(transform.position, transform.forward, collisionAvoidanceDistance, obstacleLayer))
        {
            // If there's an obstacle, set a new random destination
            SetRandomDestination();
            return; // Skip the rest of the movement logic
        }

        // Check if the agent has reached its destination
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            // Set a new random destination at the defined interval
            if (moveTimer >= randomMoveInterval)
            {
                SetRandomDestination();
                moveTimer = 0; // Reset the timer after setting a new destination
            }
        }

        // Check whether to enter idle state
        if (Random.value < idleProbability)
        {
            isIdle = true;
            idleTimer = idleDuration; // Set the idle timer
            return; // Exit this method without moving
        }

        // Calculate and apply boid behaviors
        Vector3 steering = CalculateBoidSteering();

        // Apply separation as the highest priority
        Vector3 separation = steering * separationWeight;

        // Apply idle state influence if applicable
        if (isIdle)
        {
            return; // Skip further calculations if idle
        }

        // Apply alignment behavior
        Vector3 alignment = steering.normalized * alignmentWeight;

        // Calculate the new desired velocity based on steering
        Vector3 desiredVelocity = separation + alignment;

        // Apply the new velocity as the agent's destination
        agent.SetDestination(transform.position + desiredVelocity.normalized * agent.speed);
    }

    Vector3 CalculateBoidSteering()
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;

        int nearbyCount = 0;

        // Calculate separation and alignment with nearby humans
        foreach (GameObject otherHuman in allHumans)
        {
            if (otherHuman != gameObject)
            {
                float distance = Vector3.Distance(transform.position, otherHuman.transform.position);
                if (distance < neighborRadius)
                {
                    // Separation behavior
                    if (distance < separationDistance)
                    {
                        separation += (transform.position - otherHuman.transform.position).normalized / distance;
                    }

                    // Alignment behavior
                    alignment += otherHuman.GetComponent<NavMeshAgent>().velocity;
                    nearbyCount++;
                }
            }
        }

        if (nearbyCount > 0)
        {
            // Average out the alignment vector
            alignment /= nearbyCount;

            // Normalize the behavior vectors
            return (separation + alignment).normalized;
        }

        return Vector3.zero; // No nearby humans
    }

    void SetRandomDestination()
    {
        if (game_area != null)
        {
            Vector3 randomPoint;
            int attempts = 0;

            do
            {
                // Get a random point within the bounds of the game area, with a bias towards the edges
                randomPoint = new Vector3(
                    Random.Range(game_area.transform.position.x - game_area.transform.localScale.x / 2 + 5, // Adjust to favor edges
                                 game_area.transform.position.x + game_area.transform.localScale.x / 2 - 5), // Adjust to favor edges
                    transform.position.y, // Keep the y position the same
                    Random.Range(game_area.transform.position.z - game_area.transform.localScale.z / 2 + 5, // Adjust to favor edges
                                 game_area.transform.position.z + game_area.transform.localScale.z / 2 - 5) // Adjust to favor edges
                );

                attempts++;

                // Exit if too many attempts are made to avoid an infinite loop
                if (attempts > 10) break;

            } while (!IsDestinationValid(randomPoint));

            // Set the destination for the NavMeshAgent
            agent.SetDestination(randomPoint);
        }
    }

    bool IsDestinationValid(Vector3 destination)
    {
        // Check if the destination is at least minimumDistance away from all other humans
        foreach (GameObject otherHuman in allHumans)
        {
            if (otherHuman != gameObject && Vector3.Distance(destination, otherHuman.transform.position) < minimumDistance)
            {
                return false; // The destination is too close to another human
            }
        }
        return true; // The destination is valid
    }

    void UpdateIdleState()
    {
        idleTimer -= Time.deltaTime; // Decrease the idle timer
        if (idleTimer <= 0)
        {
            isIdle = false; // Exit idle state
            SetRandomDestination(); // Set a new destination after idling
        }
    }
}
