using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;  // Import TextMeshPro namespace

public class NPCSpawner : MonoBehaviour
{
    public ObjectDatabaseSO objectDatabase;
    public int npcCount = 3;
    public Collider spawnArea;
    public float moveInterval = 2f;
    public float moveDistance = 5f;
    public float obstacleDetectionDistance = 1f;
    public float angleThreshold = 30f;
    public float rotationSpeed = 10f;
    public float movementThreshold = 0.1f;
    
    public TMP_FontAsset npcFont;  // Change from UnityEngine.Font to TMPro.TMP_FontAsset
    public Color hoverTextColor = Color.white;  // Text color when hovered
    public Color defaultTextColor = new Color(0.5f, 0.5f, 0.5f);  // Default text color (greyish)
    public Material defaultMaterial;  // Default material for the NPC
    public Material highlightMaterial;  // Material to highlight the NPC when hovered


    private List<GameObject> spawnedNPCs = new List<GameObject>();

    void Start()
    {
        SpawnNPCs();
        StartCoroutine(DespawningNPCs());
        StartCoroutine(IncreaseNPCCountOverTime());
    }

    void SpawnNPCs()
    {
        for (int i = 0; i < npcCount; i++)
        {
            SpawnSingleNPC();
        }
    }

    void SpawnSingleNPC()
    {
        ObjectData npcData = objectDatabase.objectsData[Random.Range(0, objectDatabase.objectsData.Count)];
        GameObject npcPrefab = npcData.Prefab;
        Vector3 randomPosition = GetRandomPositionInCollider();
        GameObject npc = Instantiate(npcPrefab, randomPosition, Quaternion.identity);
        spawnedNPCs.Add(npc);

        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.angularSpeed = rotationSpeed;
            agent.updateRotation = false;
        }

        AttachTextAboveNPC(npc); // Attach text above the NPC
        StartCoroutine(MoveNPC(npc));
    }

    Vector3 GetRandomPositionInCollider()
    {
        Vector3 randomPosition = Vector3.zero;
        bool positionFound = false;
        int attempts = 0;

        while (!positionFound && attempts < 10)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                spawnArea.bounds.center.y,
                Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z)
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 3.0f, NavMesh.AllAreas))
            {
                randomPosition = hit.position;
                positionFound = true;
            }

            attempts++;
        }

        return randomPosition;
    }

    IEnumerator MoveNPC(GameObject npc)
    {
        if (npc == null) yield break;

        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
        Animator animator = npc.GetComponent<Animator>();

        yield return new WaitForSeconds(Random.Range(0f, moveInterval));

        while (npc != null)
        {
            if (npc == null) yield break;

            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            Vector3 newDestination = npc.transform.position + randomDirection * moveDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(newDestination, out hit, moveDistance, NavMesh.AllAreas))
            {
                newDestination = hit.position;
            }

            if (!DetectObstacle(npc.transform.position, randomDirection))
            {
                agent.SetDestination(newDestination);
            }
            else
            {
                randomDirection = Quaternion.Euler(0, angleThreshold, 0) * randomDirection;
                newDestination = npc.transform.position + randomDirection * moveDistance;
                if (NavMesh.SamplePosition(newDestination, out hit, moveDistance, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }

            bool wasMoving = false;
            while (npc != null && (!agent.pathStatus.Equals(NavMeshPathStatus.PathComplete) ||
                    agent.remainingDistance > agent.stoppingDistance))
            {
                if (npc == null) yield break;

                bool isMoving = agent.velocity.magnitude > movementThreshold;

                if (isMoving)
                {
                    Vector3 direction = agent.velocity.normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        npc.transform.rotation = Quaternion.Slerp(
                            npc.transform.rotation,
                            targetRotation,
                            Time.deltaTime * rotationSpeed
                        );
                    }
                }

                if (isMoving != wasMoving && animator != null)
                {
                    animator.SetBool("IsWalking", isMoving);
                    wasMoving = isMoving;
                }

                yield return null;
            }

            if (animator != null && wasMoving)
            {
                animator.SetBool("IsWalking", false);
            }

            yield return new WaitForSeconds(Random.Range(0.5f, moveInterval));
        }
    }

    private bool DetectObstacle(Vector3 origin, Vector3 direction)
    {
        return Physics.Raycast(origin, direction, obstacleDetectionDistance);
    }

    IEnumerator DespawningNPCs()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 6f));

            if (spawnedNPCs.Count > 0)
            {
                int randomIndex = Random.Range(0, spawnedNPCs.Count);
                GameObject npcToDespawn = spawnedNPCs[randomIndex];

                if (npcToDespawn != null)
                {
                    spawnedNPCs.RemoveAt(randomIndex);
                    Destroy(npcToDespawn);
                }
            }
        }
    }

    public void RestoreNPCCount()
    {
        spawnedNPCs.RemoveAll(npc => npc == null);
        int missingNPCs = npcCount - spawnedNPCs.Count;

        for (int i = 0; i < missingNPCs; i++)
        {
            SpawnSingleNPC();
        }
    }

    IEnumerator IncreaseNPCCountOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 8f));
            npcCount += Random.Range(1, 2);

            for (int i = 0; i < Random.Range(1, 2); i++)
            {
                SpawnSingleNPC();
            }
        }
    }

    void AttachTextAboveNPC(GameObject npc)
    {
        // Create a TextMeshPro object for each NPC
        GameObject textObject = new GameObject("NPCNameText");
        textObject.transform.SetParent(npc.transform);  // Make it a child of the NPC

        // Add TextMeshPro component
        TextMeshPro textMeshPro = textObject.AddComponent<TextMeshPro>();

        // Set the font (use a TMP font asset, not regular Font)
        if (npcFont != null)
        {
            textMeshPro.font = npcFont;
        }
        else
        {
            Debug.LogError("Font not assigned in Inspector.");
            textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/MyFont");  // Ensure you have a TMP font in Resources
        }

        // Set the text properties
        textMeshPro.fontSize = 2f;  // Adjust the font size
        textMeshPro.alignment = TextAlignmentOptions.Center;  // Center align the text
        textMeshPro.color = defaultTextColor;  // Set default text color (greyish)

        // Set a random name for the NPC
        if (SharedGameData.Municipalities.Count > 0)
        {
            textMeshPro.text = SharedGameData.Municipalities[Random.Range(0, SharedGameData.Municipalities.Count)];
        }
        else
        {
            textMeshPro.text = "Unnamed";
        }

        // Position the text above the NPC
        textObject.transform.position = npc.transform.position + new Vector3(0f, 2f, 0f);  // Adjust height above NPC

        // Ensure the text faces the camera by rotating it
        StartCoroutine(UpdateTextRotation(textObject));

        // Attach colliders and highlight logic
        Collider npcCollider = npc.GetComponent<Collider>();
        if (npcCollider == null)
        {
            npcCollider = npc.AddComponent<BoxCollider>();  // Add a collider to the NPC if it doesn't have one
        }

        NPCHover npcHover = npc.AddComponent<NPCHover>();
        npcHover.textMeshPro = textMeshPro;
        npcHover.npcRenderer = npc.GetComponent<Renderer>();
        npcHover.defaultMaterial = defaultMaterial;
        npcHover.highlightMaterial = highlightMaterial;
    }

    // Coroutine to make the text always face the camera
    IEnumerator UpdateTextRotation(GameObject textObject)
    {
        Camera mainCamera = Camera.main;  // Get the main camera (ensure there's only one main camera in the scene)
        while (true)
        {
            if (mainCamera != null && textObject != null)
            {
                // Make the text face the camera
                textObject.transform.rotation = Quaternion.LookRotation(textObject.transform.position - mainCamera.transform.position);
            }
            yield return null;  // Wait for the next frame
        }
    }
}
    


    // FollowNPC.cs script to make the text follow the NPC
    public class FollowNPC : MonoBehaviour
    {
        public GameObject npc;

        void Update()
        {
            if (npc != null)
            {
                // Update the position of the canvas to stay above the NPC
                transform.position = npc.transform.position + new Vector3(0f, 2f, 0f);  // Adjust height above NPC
                                                                                        // Optional: Ensure it always faces the camera
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    transform.LookAt(mainCamera.transform);  // Make the canvas face the camera
                    transform.Rotate(0f, 180f, 0f);  // Correct the text rotation if needed
                }
            }
        }
    }


    public class NPCHover : MonoBehaviour
    {
        public TextMeshPro textMeshPro;
        public Renderer npcRenderer;
        public Material defaultMaterial;
        public Material highlightMaterial;

        void OnMouseEnter()
        {
            // Change text color to white on hover
            if (textMeshPro != null)
            {
                textMeshPro.color = Color.white;
            }

            // Highlight the NPC by changing its material
            if (npcRenderer != null && highlightMaterial != null)
            {
                npcRenderer.material = highlightMaterial;
            }
        }

        void OnMouseExit()
        {
            // Reset text color to default on hover exit
            if (textMeshPro != null)
            {
                textMeshPro.color = new Color(0.5f, 0.5f, 0.5f);  // Default greyish color
            }

            // Reset NPC highlight by changing back to the default material
            if (npcRenderer != null && defaultMaterial != null)
            {
                npcRenderer.material = defaultMaterial;
            }
        }
    }

