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

    [SerializeField] private int initialMinutes = 0;
    [SerializeField] private int initialSeconds = 10;
    
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
    GameOverMinutes = 1;
    GameOverSeconds = 0;

    countdownCoroutine = StartCoroutine(CountdownTimer());
}

private IEnumerator CountdownTimer()
{
    while (true)
    {
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
                    ClearSeatedObjects();
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
        gameOverText.text = "High score is 100";
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

        ClearSeatedObjects();

        // Respawn NPCs after bus arrives back
        busAnimator.SetTrigger("IsComing");

        float comingAnimationDuration = 1f;
        yield return new WaitForSeconds(comingAnimationDuration);

        busAnimator.ResetTrigger("IsComing");
        isBusPresent = true;

        // Only restore NPC count after bus arrival
        if (npcSpawner != null)
        {
            npcSpawner.RestoreNPCCount();
        }
    }



    private void ClearSeatedObjects()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        minutes = initialMinutes;
        seconds = initialSeconds;

        countdownCoroutine = StartCoroutine(CountdownTimer());

        // Use the bus GameObject reference
        if (Bus != null)
        {
            // Iterate through all children of the bus and destroy those that are NPCs
            foreach (Transform child in Bus.transform)
            {
                if (child.CompareTag("Character")) // Assuming NPCs have the tag "NPC"
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public void OnCharacterSeated(ObjectData seatedObject, GameObject objectGameObject, Vector3 position)
    {
        Vector3Int objectPositionInt = GetIntegerPosition(position);
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
                        //sit elder next to kid
                        case "kid" when newObject.type == "elder":
                        //Relocate kid
                            StartTimer(existingObject.ObjectGameObject, 0);
                            score -= 10;
                            Debug.Log("Elder next to kid.");
                            break;
                        case "adult" when newObject.type == "adult":
                            score += 20;
                            break;
                        case "police" when newObject.type == "kid":
                            Destroy(existingObject.ObjectGameObject);
                            neighborsToRemove.Add((newObject, newObjectGameObject, newPosition));
                            score -= 50;
                            break;
                        case "student" when newObject.type == "police":
                            StartTimer(existingObject.ObjectGameObject, 0);
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

// Update this method to also handle ObjectData properly when relocating
private void RelocateStoredObject()
{
    if (objectToRelocate != null)
    {
        // Store the original prefab and ObjectData before destroying
        GameObject originalPrefab = objectToRelocate;
        ObjectData originalObjectData = seatedObjects.Find(item => item.ObjectGameObject == originalPrefab).SeatedObject;

        // Destroy the object
        Destroy(objectToRelocate);
        Debug.Log($"Object {originalPrefab.name} has been destroyed.");

        objectToRelocate = null; // Clear reference to the destroyed object

        // Respawn the object on grid1 at a random position
        Vector3 randomPosition = GetRandomPositionWithinGridBounds(grid1);
        objectToRelocate = Instantiate(originalPrefab, randomPosition, Quaternion.identity);
        Debug.Log($"Object {originalPrefab.name} has been respawned at {randomPosition} on grid1.");

        // Add the relocated object back to seatedObjects with the same ObjectData
        seatedObjects.Add((originalObjectData, objectToRelocate, GetIntegerPosition(randomPosition)));
    }
    else
    {
        Debug.LogWarning("No object is set to be destroyed.");
    }
}



// Helper method to get a random position within grid bounds
private Vector3 GetRandomPositionWithinGridBounds(GameObject grid)
{
    Renderer gridRenderer = grid.GetComponent<Renderer>();

    if (gridRenderer == null)
    {
        Debug.LogError("Grid does not have a Renderer component to calculate bounds.");
        return Vector3.zero;
    }

    Bounds bounds = gridRenderer.bounds;

    float randomX = Random.Range(bounds.min.x, bounds.max.x);
    float randomZ = Random.Range(bounds.min.z, bounds.max.z);
    float y = bounds.center.y; // Assuming the object stays at the same height

    return new Vector3(randomX, y, randomZ);
}



    private void DisplayScorePanel(int score)
    {
        scoreText.text = "Score: " + score.ToString();
    }
}