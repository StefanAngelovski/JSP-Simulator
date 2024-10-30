using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridCollisionDetection : MonoBehaviour
{
    public GameObject grid1;
    public GameObject grid2;
    public float detectionRadius = 0.5f;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    private int score = 0;
    private GameObject objectToRelocate;
    private List<(ObjectData SeatedObject, GameObject ObjectGameObject, Vector3Int Position)> seatedObjects = new List<(ObjectData, GameObject, Vector3Int)>();

    public void Start()
    {
        UpdateScore(); // Initialize score display
    }

    public void OnCharacterSeated(ObjectData seatedObject, GameObject objectGameObject, Vector3 position)
    {
        Vector3Int objectPositionInt = GetIntegerPosition(position);
        seatedObjects.Add((seatedObject, objectGameObject, objectPositionInt));

        CheckPosition(seatedObject, objectPositionInt);
        CheckForNeighbors(seatedObject, objectGameObject, objectPositionInt);

        if(score < 0){
            score = 0;
        }
        UpdateScore(); // Update score whenever it changes
        Debug.Log("Position of object (integer): " + objectPositionInt);
        Debug.Log("Score is: " + score);
    }

    private void CheckPosition(ObjectData newObject, Vector3Int newPosition)
    {
        // Preferred position for different types
        if (newObject.type == "adult")
        {
            if ((newPosition.x == 7 && newPosition.y == 10) ||
                (newPosition.x == 12 && newPosition.y == 10) ||
                (newPosition.x == 7 && newPosition.y == 8) ||
                (newPosition.x == 12 && newPosition.y == 8) ||
                (newPosition.z <= 26))
            {
                score += 10;
                Debug.Log("Adult placed on the right spot.");
            }
        }

        if(newObject.type == "elder"){
            if((newPosition.z >= 29) ||
               (newPosition.x == 9 && newPosition.y == 10) ||
               (newPosition.x == 9 && newPosition.y == 8)){
                score += 10;
                Debug.Log("Elder placed on the right spot");
               }
        }

        if(newObject.type == "kid" || newObject.type == "student"){
            if((newPosition.y == 10 && newPosition.z == 28) ||
               (newPosition.y == 8 && newPosition.z == 28) ||
               (newPosition.z <= 26))
               {
                score += 10;
                Debug.Log("Kid placed on the right spot");
               }
        }

        if(newObject.type == "police"){
            if(newPosition.z <= 26){
                score += 10;
            }
        }

        if(newObject.type == "celebrity"){
                Debug.Log("Celebrity object detected with position: " + newPosition);
            if(newPosition.z >= 30 || newPosition.z <= 26){
                UpdateScore();
            }
            else{
                score += 50;
            }
        }
    }

    private void CheckForNeighbors(ObjectData newObject, GameObject newObjectGameObject, Vector3Int newPosition)
    {
        Vector3Int[] directions = {
            Vector3Int.left,   // Left
            Vector3Int.right,  // Right
            Vector3Int.back,   // Behind
            Vector3Int.forward // Upfront
        };

        List<(ObjectData, GameObject, Vector3Int)> neighborsToRemove = new List<(ObjectData, GameObject, Vector3Int)>();

        foreach (var direction in directions)
        {
            Vector3Int neighborPosition = newPosition + direction;

            foreach (var existingObject in seatedObjects)
            {
                if (existingObject.Position == neighborPosition)
                {
                    // Neighbor conditions with appropriate variables

                    if (existingObject.SeatedObject.type == "kid" && newObject.type == "elder")
                    {
                        Debug.Log("Elder next to kid");
                        score += 10; 
                    }
                    else if (existingObject.SeatedObject.type == "elder" && newObject.type == "elder")
                    {
                        score += 20;
                    }
                    else if (existingObject.SeatedObject.type == "police" && newObject.type == "elder")
                    {
                        Debug.Log("Kid placed next to elder");
                        StartTimer(newObjectGameObject, 0);
                        score += 10;
                    }
                    else if (existingObject.SeatedObject.type == "elder" && newObject.type == "kid")
                    {
                        Debug.Log("Kid placed next to elder");
                        StartTimer(newObjectGameObject, 0);
                        score += 10;
                    }
                    else if (existingObject.SeatedObject.type == "adult" && newObject.type == "adult")
                    {
                        score += 20;
                    }
                    else if (existingObject.SeatedObject.type == "kid" && newObject.type == "police")
                    {
                        Destroy(newObjectGameObject); 
                        neighborsToRemove.Add((newObject, newObjectGameObject, newPosition));
                        score -= 50;
                    }
                    else if (existingObject.SeatedObject.type == "student" && newObject.type == "police")
                    {
                        Debug.Log("Police next to student");
                        StartTimer(existingObject.ObjectGameObject ,0);
                        score -= 20;
                    }
                }
            }
        }

        foreach (var neighbor in neighborsToRemove)
        {
            seatedObjects.Remove(neighbor);
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

    private void StartTimer(GameObject existingObject, int timer)
    {
        objectToRelocate = existingObject; 
        Invoke(nameof(RelocateStoredObject), timer); 
    }

    private void RelocateStoredObject()
    {
        if (objectToRelocate != null)
        {
            Vector3Int newPosition = GetRandomPositionWithinGrid();
            objectToRelocate.transform.position = newPosition;
            Debug.Log($"Object {objectToRelocate.name} relocated to {newPosition}");
        }
    }

    private Vector3Int GetRandomPositionWithinGrid()
    {
        GameObject selectedGrid = Random.value > 0.5f ? grid1 : grid2;
        Renderer gridRenderer = selectedGrid.GetComponent<Renderer>();
        Bounds gridBounds = gridRenderer.bounds;

        int x = Mathf.RoundToInt(Random.Range(gridBounds.min.x, gridBounds.max.x));
        int y = Mathf.RoundToInt(Random.Range(gridBounds.min.y, gridBounds.max.y));
        int z = Mathf.RoundToInt(Random.Range(gridBounds.min.z, gridBounds.max.z));

        return new Vector3Int(x, y, z);
    }

    private void UpdateScore()
    {
        scoreText.text = "Score: " + score.ToString();
    }
}
