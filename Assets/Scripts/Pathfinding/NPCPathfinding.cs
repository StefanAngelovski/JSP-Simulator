using UnityEngine;
using UnityEngine.AI;

public class NPCPathfinding : MonoBehaviour
{
    [SerializeField] private Transform movePositionTransform;
    [SerializeField] private float walkingSpeedThreshold = 0.1f; // Speed threshold to toggle IsWalking
    [SerializeField] private float rotationSpeed = 5f; // Speed at which the object rotates to face its direction

    private NavMeshAgent navMeshAgent;
    private Animator animator;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Get the Animator component on the same GameObject
    }

    void Update()
    {
        // Set the destination of the NavMeshAgent
        navMeshAgent.destination = movePositionTransform.position;
        
        // Check the speed of the NavMeshAgent
        float speed = navMeshAgent.velocity.magnitude;

        // Set the IsWalking parameter based on the speed threshold
        bool isWalking = speed > walkingSpeedThreshold;
        animator.SetBool("IsWalking", isWalking);

        // Rotate towards the movement direction if moving
        if (isWalking)
        {
            Vector3 direction = navMeshAgent.velocity.normalized; // Get the direction of movement
            if (direction != Vector3.zero) // Ensure there is a direction to face
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
