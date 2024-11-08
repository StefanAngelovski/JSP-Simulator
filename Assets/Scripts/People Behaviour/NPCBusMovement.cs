using UnityEngine;

public class NPCBusMovement : MonoBehaviour
{
    private Vector3 initialGridOffset;
    private Transform busTransform;
    private Transform gridTransform;
    private bool isInitialized = false;
    private bool isSeated = false;
    private Vector3 seatOffset;

    public void Initialize(Transform bus, Transform grid, Vector3 exactPlacementPosition)
    {
        busTransform = bus;
        gridTransform = grid;

        // Store the exact offset from the grid using the placement position
        initialGridOffset = exactPlacementPosition - bus.position;
        transform.position = exactPlacementPosition; // Set initial position exactly
        isInitialized = true;
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        // Update position relative to bus movement while maintaining exact offset
        Vector3 targetPosition = busTransform.position + initialGridOffset;

        // Apply seat offset if the NPC is seated
        if (isSeated)
        {
            targetPosition += seatOffset;
        }

        transform.position = targetPosition;
    }

    public void UpdateSeatedState(bool seated, Vector3 seatPositionOffset = default)
    {
        isSeated = seated;
        if (seated)
        {
            seatOffset = seatPositionOffset;
            // Update the initial offset to include the seat position
            initialGridOffset += seatPositionOffset;
        }
        else
        {
            // Remove the seat offset from the initial offset
            initialGridOffset -= seatOffset;
            seatOffset = Vector3.zero;
        }
    }
}