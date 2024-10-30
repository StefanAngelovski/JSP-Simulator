using UnityEngine;

public class NPCClickHandler : MonoBehaviour
{
    [SerializeField]
    private ObjectDatabaseSO objectDatabase; // Assign this in the Inspector

    private void OnMouseDown()
    {
        // Log which NPC was clicked
        Debug.Log("Clicked on NPC: " + gameObject.name);

        // Get the Placement script
        Placement placement = FindFirstObjectByType<Placement>();
        if (placement == null)
        {
            Debug.LogError("Placement script not found! Ensure it is attached to an active GameObject.");
            return; // Exit if placement is not found
        }

        // Ensure the objectDatabase is assigned
        if (objectDatabase == null)
        {
            Debug.LogError("ObjectDatabaseSO reference is not set in NPCClickHandler!");
            return; // Exit if the database reference is not set
        }

        // Log the count of objects in the database
        Debug.Log("ObjectDatabase contains " + objectDatabase.objectsData.Count + " entries.");

        // Find NPC data based on the clicked prefab
        ObjectData npcData = objectDatabase.objectsData.Find(data => data.Prefab.name == gameObject.name.Replace("(Clone)", ""));
        if (npcData != null)
        {
            placement.StartPlacement(npcData.ID); // Pass the NPC ID to the Placement script
            Debug.Log("Successfully clicked on NPC: " + npcData.Name); // Log the NPC name or ID
        }
        else
        {
            Debug.LogWarning("NPC data not found for the clicked object: " + gameObject.name);
            foreach (var data in objectDatabase.objectsData)
            {
                Debug.Log("Available prefab in ObjectDatabase: " + data.Prefab.name);
            }
        }
    }
}
