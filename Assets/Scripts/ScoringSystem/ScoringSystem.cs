using System.Collections;
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

    public Button BusButton;
    public TextMeshProUGUI timerText;
    private int minutes;
    private int seconds;
    private Coroutine countdownCoroutine; 

    private List<(ObjectData SeatedObject, GameObject ObjectGameObject, Vector3Int Position)> seatedObjects = new List<(ObjectData, GameObject, Vector3Int)>();

    public void Start()
    {
        gameOverPanel.SetActive(false);
        Vector3Int grid1PositionInt = GetIntegerPosition(grid1.transform.position);
        Vector3Int grid2PositionInt = GetIntegerPosition(grid2.transform.position);

        Debug.Log("Grid1 integer position: " + grid1PositionInt);
        Debug.Log("Grid2 integer position: " + grid2PositionInt);

        if (BusButton != null)
        {
            BusButton.onClick.AddListener(ClearSeatedObjects);
        }

        // Start the countdown timer coroutine
        countdownCoroutine = StartCoroutine(CountdownTimer());
    }

     private IEnumerator CountdownTimer()
    {
        minutes = 1; 
        seconds = 0;

        while (true)
        {
            timerText.text = string.Format("Time until new bus arrives: {0:00}:{1:00}", minutes, seconds);
            yield return new WaitForSeconds(1);

            seconds--;

            if (seconds < 0)
            {
                seconds = 59;
                minutes--;
                if (minutes < 0)
                {
                    minutes = 0; 
                    seconds = 0; 
                    ClearSeatedObjects();
                    minutes = 1;
                }
            }
        }
    }

    private void ClearSeatedObjects()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        minutes = 1;
        seconds = 0;

        countdownCoroutine = StartCoroutine(CountdownTimer());

        foreach (var item in seatedObjects)
        {
            Destroy(item.ObjectGameObject);
        }

        seatedObjects.Clear();

        Debug.Log("seatedObjects list has been cleared, and all associated GameObjects have been destroyed.");
    }

    public void OnCharacterSeated(ObjectData seatedObject, GameObject objectGameObject, Vector3 position)
    {
        Vector3Int objectPositionInt = GetIntegerPosition(position);
        seatedObjects.Add((seatedObject, objectGameObject, objectPositionInt));

        CheckPosition(seatedObject, objectPositionInt);

        CheckForNeighbors(seatedObject, objectGameObject, objectPositionInt);

        if (score < 0)
        {
            score = 0;
        }
        Debug.Log("Position of object (integer): " + objectPositionInt);
        Debug.Log("score is:" + score);
    }

    private void CheckPosition(ObjectData newObject, Vector3Int newPosition)
    {
        //prefered position window and back?
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

        //prefered position front and aisle
        if (newObject.type == "elder")
        {
            if ((newPosition.z >= 29) ||
                (newPosition.x == 9 && newPosition.y == 10) ||
                (newPosition.x == 9 && newPosition.y == 8))
            {
                score += 10;
                Debug.Log("Elder placed on the right spot");
            }
        }

        //prefered middle and back
        if (newObject.type == "kid" || newObject.type == "student")
        {
            if ((newPosition.y == 10 && newPosition.z == 28) ||
                (newPosition.y == 8 && newPosition.z == 28) ||
                (newPosition.z <= 26))
            {
                score += 10;
                Debug.Log("Kid placed on the right spot");
            }
        }

        if (newObject.type == "police")
        {
            if (newPosition.z <= 26)
            {
                score += 10;
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
                    //if you place an elder next to a kid
                    if (existingObject.SeatedObject.type == "kid" && newObject.type == "elder")
                    {
                        Debug.Log("elder next to kid");
                        score += 10; 
                    }
                    
                    //if you place two elders next to eachother
                    else if(existingObject.SeatedObject.type == "elder" && newObject.type == "elder")
                    {
                        score += 20;
                    }

                    //if you place an elder next to police
                    else if(existingObject.SeatedObject.type == "police" && newObject.type == "elder")
                    {
                        Debug.Log("kid placed next to elder");
                        StartTimer(newObjectGameObject, 0);
                        score += 10;
                    }

                    //if you place a kid next to an elder (wait for the kid to change seats and dedact the score)
                    else if(existingObject.SeatedObject.type == "elder" && newObject.type == "kid")
                    {
                        Debug.Log("kid placed next to elder");
                        StartTimer(newObjectGameObject, 0);
                        score += 10;
                    }

                    //if you place two adults next to eachother
                    else if(existingObject.SeatedObject.type == "adult" && newObject.type == "adult")
                    {
                        score += 20;
                    }

                    //if you place a police next to a kid (kid leaves the bus)
                    else if(existingObject.SeatedObject.type == "kid" && newObject.type == "police")
                    {
                        Destroy(existingObject.ObjectGameObject); 
                        neighborsToRemove.Add((newObject, newObjectGameObject, newPosition));
                        score -= 50;
                    }

                    //if you place a police next to a student (replace the location for the student)
                    else if(existingObject.SeatedObject.type == "student" && newObject.type == "police")
                    {
                        Debug.Log("police next to student");
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

    private void GameOver(int score){
        scoreText.text = "Score: " + score.ToString();
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);
    }
}