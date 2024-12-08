using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoringSystem : MonoBehaviour
{
    public GameObject grid1;
    public GameObject grid2;
    public float detectionRadius = 0.5f;
    public TextMeshProUGUI scoreText;
    public GameObject scorePanel;
    private int score = 0;
    private GameObject objectToRelocate;
    //Game Over
    public GameObject gameOver;
    public TextMeshProUGUI gameOverText;
    private int GameOverMinutes;
    private int GameOverSeconds;

    public TextMeshProUGUI timerText;

    [SerializeField] private int initialMinutes = 2;
    [SerializeField] private int initialSeconds = 0;
    
    private int minutes;
    private int seconds;
    private Coroutine countdownCoroutine;

    public GameObject Bus;
    private Animator busAnimator;

    private bool isBusLeaving = false;
    private bool isBusPresent = true;
    public NPCSpawner npcSpawner; 

    private List<(ObjectData SeatedObject, GameObject ObjectGameObject, Vector3Int Position)> seatedObjects = new List<(ObjectData, GameObject, Vector3Int)>();

  public void Start()
{
    score = 0;

    if (Bus != null)
    {
        busAnimator = Bus.GetComponent<Animator>();
    }

    // Initialize timer values
    minutes = initialMinutes;
    seconds = initialSeconds;

    // Initialize GameOver timer
    GameOverMinutes = 2;
    GameOverSeconds = 0;

    countdownCoroutine = StartCoroutine(CountdownTimer());
}

private IEnumerator CountdownTimer()
{
    while (true)
    {

        if(npcSpawner.npcCount >= 10){
            HandleGameOver();
        }
            timerText.text = $"Shift ends in: {GameOverMinutes:00}:{GameOverSeconds:00}";

        yield return new WaitForSeconds(1);

        // Countdown for the bus timer
        seconds--;
        if (seconds < 0)
        {
            seconds = 59;
            minutes--;
            if (minutes < 0)
            {
                minutes = initialMinutes;
                seconds = initialSeconds;

                if (busAnimator != null && isBusPresent && !isBusLeaving)
                {
                    StartCoroutine(HandleBusDeparture());
                }
            }
        }

        // Countdown for the GameOver timer
        GameOverSeconds--;
        if (GameOverSeconds < 0)
        {
            GameOverSeconds = 59;
            GameOverMinutes--;
            if (GameOverMinutes < 0)
            {
                // Trigger Game Over logic
                HandleGameOver();
                yield break;
            }
        }

        if(npcSpawner != null){
            npcSpawner.RestoreNPCCount();
        }
    }
}

private void HandleGameOver()
{
    // Stop all game activity
    Time.timeScale = 0;

    // Display the Game Over panel
    if (gameOver != null)
    {
        gameOver.SetActive(true);
    }

    // Set the Game Over text
    if (gameOverText != null)
    {
        gameOverText.text = "High score is "+score;
    }

    Debug.Log("Game Over triggered. High score is 100.");
}

    private IEnumerator HandleBusDeparture()
    {
        isBusLeaving = true;
        isBusPresent = false;

        busAnimator.ResetTrigger("IsComing");
        busAnimator.SetTrigger("IsLeaving");

        float leavingAnimationDuration = 1f;
        yield return new WaitForSeconds(leavingAnimationDuration);

        busAnimator.ResetTrigger("IsLeaving");
        isBusLeaving = false;

        float delayBetweenBuses = 0f;
        yield return new WaitForSeconds(delayBetweenBuses);

        ClearAllObjects();

        // Respawn NPCs after bus arrives back
        busAnimator.SetTrigger("IsComing");

        float comingAnimationDuration = 1f;
        yield return new WaitForSeconds(comingAnimationDuration);

        busAnimator.ResetTrigger("IsComing");
        isBusPresent = true;
    }



    private void ClearAllObjects()
    {

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        minutes = initialMinutes;
        seconds = initialSeconds;

        countdownCoroutine = StartCoroutine(CountdownTimer());

        if (Bus != null)
        {
            foreach (Transform child in Bus.transform)
            {
                if (child.CompareTag("Character")) 
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public void OnCharacterSeated(ObjectData seatedObject, GameObject objectGameObject, Vector3 position)
    {
        Vector3Int objectPositionInt = GetIntegerPosition(position);
        // Pass `seatedObject` directly instead of trying to retrieve it later
        seatedObjects.Add((seatedObject, objectGameObject, objectPositionInt));

        CheckPosition(seatedObject, objectPositionInt);
        CheckForNeighbors(seatedObject, objectGameObject, objectPositionInt);

        score = Mathf.Max(score, 0);
    }


    private void CheckPosition(ObjectData newObject, Vector3Int newPosition)
    {
        // Position preference handling
        // Adult seat preferences
        if (newObject.type == "adult" &&
            ((newPosition.x == 7 || newPosition.x == 12) && (newPosition.y == 10 || newPosition.y == 8) ||
             newPosition.z <= 26))
        {
            score += 10;
            Debug.Log("Adult placed on the right spot.");
        }

        // Elder seat preferences
        if (newObject.type == "elder" &&
            (newPosition.z >= 29 || (newPosition.x == 9 && (newPosition.y == 10 || newPosition.y == 8))))
        {
            score += 10;
            Debug.Log("Elder placed on the right spot.");
        }

        // Kid or student preferences
        if ((newObject.type == "kid" || newObject.type == "student") &&
            ((newPosition.y == 10 || newPosition.y == 8) && newPosition.z == 28 || newPosition.z <= 26))
        {
            score += 10;
            Debug.Log("Kid placed on the right spot.");
        }

        // Police preferences
        if (newObject.type == "police" && newPosition.z <= 26)
        {
            score += 10;
        }

        DisplayScorePanel(score);
    }

    private void CheckForNeighbors(ObjectData newObject, GameObject newObjectGameObject, Vector3Int newPosition)
    {
        Vector3Int[] directions = {
            Vector3Int.left, Vector3Int.right, Vector3Int.back, Vector3Int.forward
        };

        List<(ObjectData, GameObject, Vector3Int)> neighborsToRemove = new List<(ObjectData, GameObject, Vector3Int)>();

        foreach (var direction in directions)
        {
            Vector3Int neighborPosition = newPosition + direction;

            foreach (var existingObject in seatedObjects)
            {
                if (existingObject.Position == neighborPosition)
                {
                    switch (existingObject.SeatedObject.type)
                    {
                        //police -> elder == make the police relocate
                        case "police" when newObject.type == "elder":
                            StartTimer(existingObject.ObjectGameObject, existingObject.SeatedObject, Random.Range(0,1));
                            Destroy(existingObject.ObjectGameObject);
                            neighborsToRemove.Add((newObject, newObjectGameObject, newPosition));
                            score += 20;
                            break;

                        //elder -> elder
                        case "elder" when newObject.type == "elder":
                            score += 5;
                            break;
                        
                        //kid -> elder == make the kid relocate
                            case "kid" when newObject.type == "elder":
                                StartTimer(existingObject.ObjectGameObject, existingObject.SeatedObject, Random.Range(0,1));
                                Destroy(existingObject.ObjectGameObject);
                                neighborsToRemove.Add((newObject, newObjectGameObject, newPosition));
                                score -= 20;
                                break;

                        //elder -> student == make the student relocate with a delay
                            case "student" when newObject.type == "elder":
                                int number = Random.Range(0,5);
                                if(number >= 2){
                                Destroy(existingObject.ObjectGameObject);
                                neighborsToRemove.Add((newObject, newObjectGameObject, newPosition));
                                }
                                else{
                                StartTimer(existingObject.ObjectGameObject, existingObject.SeatedObject, Random.Range(2,3));
                                }
                                score -= 15;
                                break;
                        
                        //elder -> adult == make the adult relocate but give points
                            case "elder" when newObject.type == "adult":
                                StartTimer(existingObject.ObjectGameObject, existingObject.SeatedObject, Random.Range(0,1));
                                score += 15;
                                break;
                        //kid -> police == make the police relocate
                        case "kid" when newObject.type == "police":
                            StartTimer(existingObject.ObjectGameObject, existingObject.SeatedObject, Random.Range(0,1));
                            score -= 5;
                            break;

                        case "police" when newObject.type == "kid":
                            StartTimer(existingObject.ObjectGameObject, existingObject.SeatedObject, Random.Range(0,1));
                            score -= 15;
                            break;

                        case "student" when newObject.type == "police":
                            StartTimer(existingObject.ObjectGameObject, existingObject.SeatedObject, Random.Range(2,3));
                            Destroy(existingObject.ObjectGameObject);
                            neighborsToRemove.Add((newObject, newObjectGameObject, newPosition));
                            score -= 20;
                            Debug.Log("Police next to student.");
                            break;
                    }
                }
            }
        }

        foreach (var neighbor in neighborsToRemove)
        {
            seatedObjects.Remove(neighbor);
        }

        DisplayScorePanel(score);
    }


private Vector3 GetRandomAdjacentPosition(Vector3 position)
{
    // Define possible directions to find adjacent positions (left, right, up, down, forward, back)
    Vector3[] directions = {
        Vector3.left, Vector3.right, Vector3.forward, Vector3.back, Vector3.up, Vector3.down
    };

    // Pick a random direction
    Vector3 randomDirection = directions[Random.Range(0, directions.Length)];

    // Return the new adjacent position
    return position + randomDirection;
}


    private Vector3Int GetIntegerPosition(Vector3 position)
    {
        return new Vector3Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y),
            Mathf.RoundToInt(position.z)
        );
    }

private void StartTimer(GameObject existingObject, ObjectData objectData, int timer)
{
    objectToRelocate = existingObject;
    Invoke(nameof(RelocateStoredObject), timer);
}

private void RelocateStoredObject()
{
    if (objectToRelocate != null)
    {
        // Ensure grids are properly configured
        if (grid1 == null || grid2 == null)
        {
            Debug.LogWarning("Grids are not assigned. Cannot relocate object.");
            return;
        }

        // Find the matching ObjectData in seatedObjects
        var objectDataEntry = seatedObjects.Find(item => item.ObjectGameObject == objectToRelocate);
        if (objectDataEntry.Equals(default((ObjectData, GameObject, Vector3Int))))
        {
            Debug.LogError("Failed to find ObjectData for relocation.");
            return;
        }

        // Select a random grid
        GameObject selectedGrid = Random.value > 0.5f ? grid1 : grid2;

        // Calculate a new random position within the bounds of the grid
        Vector3 newPosition = GetRandomPositionWithinGridBounds(selectedGrid);

        // Destroy the existing object
        seatedObjects.RemoveAll(item => item.ObjectGameObject == objectToRelocate);
        Destroy(objectToRelocate);

        // Create a new instance of the object at the new position
        GameObject newObject = Instantiate(objectToRelocate, newPosition, Quaternion.identity);
        
        //Parent the new object to the Bus GameObject
        newObject.transform.SetParent(Bus.transform);

        // Add the new object with the same ObjectData to seatedObjects
        seatedObjects.Add((objectDataEntry.SeatedObject, newObject, GetIntegerPosition(newPosition)));

        Debug.Log($"Object relocated to {newPosition}");
    }
}




private Vector3 GetRandomPositionWithinGridBounds(GameObject grid)
{
    Renderer gridRenderer = grid.GetComponent<Renderer>();

    // Ensure the grid has a renderer to determine its bounds
    if (gridRenderer == null)
    {
        Debug.LogError("Grid does not have a Renderer component.");
        return Vector3.zero;
    }

    Bounds bounds = gridRenderer.bounds;

    // Generate a random position within the grid's bounds
    float randomX = Random.Range(bounds.min.x, bounds.max.x);
    float randomY = bounds.min.y; // Assuming the grid is flat on Y-axis
    float randomZ = Random.Range(bounds.min.z, bounds.max.z);

    return new Vector3(randomX, randomY, randomZ);
}


    private void DisplayScorePanel(int score)
    {
        scoreText.text = "Score: " + score.ToString();
    }
}