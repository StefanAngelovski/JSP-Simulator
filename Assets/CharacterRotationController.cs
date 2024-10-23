using UnityEngine;

public class CharacterRotationController : MonoBehaviour
{
    private Animator animator;

    // Public field to set the left-facing rotation angle in the Inspector
    public Vector3 seatedDirection = new Vector3(0, -90, 0); // Default to face left

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Check if the character is walking
        if (animator.GetBool("IsWalking"))
        {
            // Face the direction of movement
            FaceMovementDirection();
        }
        // Check if the character is seated
        else if (animator.GetBool("IsSeated"))
        {
            // Face the specified seated direction
            FaceLeft();
        }
    }

    private void FaceMovementDirection()
    {
        // Assuming you have a Rigidbody or any other way to get movement direction
        // Replace this with your actual movement logic
        Vector3 movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (movementDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
        }
    }

    private void FaceLeft()
    {
        // Set the rotation to the specified seated direction
        transform.rotation = Quaternion.Euler(seatedDirection);
    }
}
