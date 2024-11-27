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

    [SerializeField] private int initialMinutes = 1;
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
                        case "kid" when newObject.type == "elder":
                        case "elder" when newObject.type == "kid":
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

private void RelocateStoredObject()
{
    if (objectToRelocate != null)
    {
        // Ensure the grids are properly configured
        if (grid1 == null || grid2 == null)
        {
            Debug.LogWarning("Grids are not assigned. Cannot relocate object.");
            return;
        }

        // Select a random grid
        GameObject selectedGrid = Random.value > 0.5f ? grid1 : grid2;

        // Calculate a new random position within the bounds of the grid
        Vector3 newPosition = GetRandomPositionWithinGridBounds(selectedGrid);

        // Move the object to the new position
        objectToRelocate.transform.position = newPosition;

        Debug.Log($"Object {objectToRelocate.name} relocated to {newPosition}");
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