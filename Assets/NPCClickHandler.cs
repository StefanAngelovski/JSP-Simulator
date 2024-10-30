using UnityEngine;

public class NPCClickHandler : MonoBehaviour
{
    [SerializeField]
    private ObjectDatabaseSO objectDatabase; // Assign this in the Inspector

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

        // Find the NPC data based on the clicked NPC
        ObjectData npcData = objectDatabase.objectsData.Find(data => data.Prefab.name == gameObject.name.Replace("(Clone)", ""));
        if (npcData != null)
        {
            // Call the placement function to start placing the NPC
            placement.StartPlacement(npcData.ID); 
        }
    }
}
