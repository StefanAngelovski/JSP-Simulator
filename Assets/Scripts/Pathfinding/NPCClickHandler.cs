using UnityEngine;

public class NPCClickHandler : MonoBehaviour
{
    [SerializeField]
    private ObjectDatabaseSO objectDatabase;

    private void OnMouseDown()
    {
        Placement placement = FindFirstObjectByType<Placement>();
        if (placement == null)
        {
            Debug.LogError("Placement script not found! Ensure it is attached to an active GameObject.");
            return; 
        }

        if (objectDatabase == null)
        {
            Debug.LogError("ObjectDatabaseSO reference is not set in NPCClickHandler!");
            return; 
        }

        placement.StartPlacement(gameObject);
    }
}
