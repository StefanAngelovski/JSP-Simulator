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
        scorePanel.SetActive(false);

        if (Bus != null)
        {
            busAnimator = Bus.GetComponent<Animator>();
        }

        minutes = initialMinutes;
        seconds = initialSeconds;

        countdownCoroutine = StartCoroutine(CountdownTimer());
    }

    private IEnumerator CountdownTimer()
    {
        while (true)
        {
            if (isBusLeaving)
            {
                timerText.text = $"Time until next bus arrives: {minutes:00}:{seconds:00}";
            }
            else if (isBusPresent)
            {
                timerText.text = $"Bus leaves in: {minutes:00}:{seconds:00}";
            }
            else
            {
                timerText.text = $"Time until next bus arrives: {minutes:00}:{seconds:00}";
            }

            yield return new WaitForSeconds(1);

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
        }
    }

    private IEnumerator HandleBusDeparture()
    {
        isBusLeaving = true;
        isBusPresent = false;

        busAnimator.ResetTrigger("IsComing");
        busAnimator.SetTrigger("IsLeaving");

        float leavingAnimationDuration = 4f;
        yield return new WaitForSeconds(leavingAnimationDuration);

        busAnimator.ResetTrigger("IsLeaving");
        isBusLeaving = false;

        float delayBetweenBuses = 4f;
        yield return new WaitForSeconds(delayBetweenBuses);

        ClearSeatedObjects();

        // Respawn NPCs after bus arrives back
        busAnimator.SetTrigger("IsComing");

        float comingAnimationDuration = 4f;
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

        foreach (var item in seatedObjects)
        {
            Destroy(item.ObjectGameObject);
        }

        seatedObjects.Clear();

        Debug.Log("Seated objects list has been cleared, and all associated GameObjects have been destroyed.");
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
                            StartTimer(newObjectGameObject, 0);
                            score += 10;
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

    private void DisplayScorePanel(int score)
    {
        scoreText.text = "Score: " + score.ToString();
        Time.timeScale = 0;
        scorePanel.SetActive(true);
    }
}