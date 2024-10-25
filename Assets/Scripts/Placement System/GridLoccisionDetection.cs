using System.Collections.Generic;
using UnityEngine;

public class GridCollisionDetection : MonoBehaviour
{
    public GameObject grid1;
    public GameObject grid2;
    public float detectionRadius = 0.5f;
    private int counter = 0;

    private List<(ObjectData SeatedObject, Vector3Int Position)> seatedObjects = new List<(ObjectData, Vector3Int)>();

    public void Start()
    {
        Vector3Int grid1PositionInt = GetIntegerPosition(grid1.transform.position);
        Vector3Int grid2PositionInt = GetIntegerPosition(grid2.transform.position);

        Debug.Log("Grid1 integer position: " + grid1PositionInt);
        Debug.Log("Grid2 integer position: " + grid2PositionInt);
    }

    public void OnCharacterSeated(ObjectData seatedObject, Vector3 position)
    {
        Vector3Int objectPositionInt = GetIntegerPosition(position);
        seatedObjects.Add((seatedObject, objectPositionInt));

        CheckPosition(seatedObject, objectPositionInt);
        // Check neighbors for the newly seated object
        CheckForNeighbors(seatedObject, objectPositionInt);

        Debug.Log("Position of object (integer): " + objectPositionInt);
    }


    private void CheckPosition(ObjectData newObject, Vector3 newPosition){
        Debug.Log(newObject.type);
        if(newObject.type == "adult"){
            if((newPosition.x == 7 && newPosition.y == 10) || (newPosition.x == 12 && newPosition.y == 10) || (newPosition.x == 6 && newPosition.y == 7) || (newPosition.x == 11 && newPosition.y == 7) || (newPosition.z <= 26))
                {
                    Debug.Log("Adult placed on the right spot");
                }
        }
    }

    private void CheckForNeighbors(ObjectData newObject, Vector3Int newPosition)
    {
        Vector3Int[] directions = {
            Vector3Int.left,   // Left
            Vector3Int.right,  // Right
            Vector3Int.back,   // Behind
            Vector3Int.forward // Upfront
        };

        foreach (var direction in directions)
        {
            Vector3Int neighborPosition = newPosition + direction;

            // Check each object in seatedObjects to see if it's in the neighbor position
            foreach (var existingObject in seatedObjects)
            {
                if (existingObject.Position == neighborPosition)
                {
                    Debug.Log($"The object {newObject.Name} has a neighbor {existingObject.SeatedObject.Name} of type {existingObject.SeatedObject.type} at {direction} position.");
                }
            }
        }
    }

    private Vector3Int GetIntegerPosition(Vector3 position)
    {
        return new Vector3Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y),
            Mathf.RoundToInt(position.z)
        );
    }
}


                    // if(objectType == "elder"){
                    //     //aisle and front prefered
                    //     if(z >= 29 || (x == 8 && y == 10) || (x == 8 && y == 7)){
                    //         Debug.Log("Prefered Elder");
                    //         score += 2;
                    //         break;
                    //     }
                    //     else{
                    //         score--;
                    //         break;
                    //     }
                    // }

                    // if(objectType == "kid"){
                    //     //middle and back lower row
                    //     if((y == 10 && z == 28) || (y == 7 && z == 25)){
                    //         Debug.Log("Prefered Kid");
                    //         score += 2;
                    //         break;
                    //     }
                    //     else{
                    //         score--;
                    //         break;
                    //     }
                    // }