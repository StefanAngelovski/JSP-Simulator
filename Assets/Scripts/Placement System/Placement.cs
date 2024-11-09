using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Placement : MonoBehaviour
{
    [SerializeField]
    private GameObject mouseIndicator, cellIndicator;
    private Renderer cellIndicatorRenderer;
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private ObjectDatabaseSO database;
    private int selectedObjectIndex = -1;

    [SerializeField]
    private GameObject gridVisualisationTop;
    [SerializeField]
    private GameObject gridVisualisationBottom;

    private GridData gridData;

    private List<GameObject> placedGameObjects = new();

    [SerializeField]
    private GameObject emptyPreviewObject;
    private GameObject previewObject;
    private Renderer[] previewObjectRenderers;

    [SerializeField]
    private Material previewObjectMaterialValid;
    [SerializeField]
    private Material previewObjectMaterialInvalid;

    [SerializeField]
    private GameObject bus;

    private int rotation = 0;
    private Vector3Int objectOffset;

    private GameObject originalNPC;

    [SerializeField]
    private float topGridHeightOffset = 0.1f;
    [SerializeField]
    private float bottomGridHeightOffset = 0.1f;

    private PeopleCounter peopleCounter;

    private void Start()
    {
        previewObject = emptyPreviewObject;
        StopPlacement();
        gridData = new();
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
        
        peopleCounter = FindFirstObjectByType<PeopleCounter>();
        if (peopleCounter == null)
        {
            Debug.LogWarning("PeopleCounter not found in scene!");
        }
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex < 0)
        {
            return;
        }

        gridVisualisationTop.SetActive(true);
        gridVisualisationBottom.SetActive(true);
        cellIndicator.SetActive(true);

        previewObject.SetActive(true);
        previewObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
        previewObjectRenderers = previewObject.GetComponentsInChildren<Renderer>();

        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
        inputManager.OnRotate += RotateStructure;
    }

    public void StartPlacement(GameObject npc)
    {
        StopPlacement();

        ObjectData npcData = database.objectsData.Find(data => data.Prefab.name == npc.name.Replace("(Clone)", ""));
        if (npcData == null)
        {
            Debug.LogError("NPC data not found in database!");
            return;
        }

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == npcData.ID);
        originalNPC = npc;

        gridVisualisationTop.SetActive(true);
        gridVisualisationBottom.SetActive(true);
        cellIndicator.SetActive(true);

        previewObject = Instantiate(npc);
        previewObjectRenderers = previewObject.GetComponentsInChildren<Renderer>();

        NavMeshAgent previewAgent = previewObject.GetComponent<NavMeshAgent>();
        if (previewAgent != null)
        {
            Destroy(previewAgent);
        }

        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
        inputManager.OnRotate += RotateStructure;
    }

    private void RotateStructure()
    {
        if (rotation < 3)
            rotation += 1;
        else
            rotation = 0;

        switch (rotation)
        {
            case 0:
                objectOffset = new Vector3Int(0, 0, 0);
                break;
            case 1:
                objectOffset = new Vector3Int(0, 0, 1);
                break;
            case 2:
                objectOffset = new Vector3Int(1, 0, 1);
                break;
            case 3:
                objectOffset = new Vector3Int(1, 0, 0);
                break;
        }

        previewObject.transform.rotation = Quaternion.Euler(0, rotation * 90, 0);
        previewObject.transform.position += grid.CellToWorld(objectOffset);
    }

    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI() || inputManager.IsPointerOverNPC())
            return;

        Vector3 mousePosition = inputManager.GetMousePositionOnGrid();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        Vector3Int placePosition = gridPosition + objectOffset;

        bool placementValidity = gridData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size, rotation);

        if (!placementValidity)
            return;

        bool isOverChair = CheckPreviewOverChair(gridPosition);
        Vector3 exactPosition = CalculateObjectPosition(placePosition, isOverChair);

        GameObject newObject;
        if (originalNPC != null)
        {
            newObject = Instantiate(originalNPC);
            NavMeshAgent navMeshAgent = newObject.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                Destroy(navMeshAgent);
            }
            if (peopleCounter != null)
            {
                peopleCounter.IncrementCount();
            }
            Destroy(originalNPC);
        }
        else
        {
            newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
            NavMeshAgent agent = newObject.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                Destroy(agent);
            }
            if (peopleCounter != null && database.objectsData[selectedObjectIndex].Prefab.CompareTag("NPC"))
            {
                peopleCounter.IncrementCount();
            }
        }

        newObject.transform.rotation = Quaternion.Euler(0, rotation * 90, 0);
        newObject.transform.position = exactPosition;

        NPCBusMovement busMovement = newObject.AddComponent<NPCBusMovement>();
        busMovement.Initialize(bus.transform, grid.transform, exactPosition);

        placedGameObjects.Add(newObject);

        gridData.AddObjectAt(gridPosition,
            rotation,
            database.objectsData[selectedObjectIndex].Size,
            database.objectsData[selectedObjectIndex].ID,
            placedGameObjects.Count - 1);

        if (isOverChair)
        {
            CheckAndSetSitting(newObject, placePosition);
        }

        originalNPC = null;
        StopPlacement();
    }

    private void CheckAndSetSitting(GameObject placedObject, Vector3Int gridPosition)
    {
        Vector3 center = grid.CellToWorld(gridPosition);
        bool isPlacingOnTop = gridPosition.y > 0;

        center.y += isPlacingOnTop ? topGridHeightOffset : bottomGridHeightOffset;
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f);

        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Chair"))
            {
                Vector3 adjustedPosition = grid.CellToWorld(gridPosition);
                adjustedPosition.y = placedObject.transform.position.y;

                placedObject.transform.position = adjustedPosition;

                Animator animator = placedObject.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetBool("IsSeated", true);
                    
                    NPCBusMovement busMovement = placedObject.GetComponent<NPCBusMovement>();

                    ScoringSystem scoringSystem = FindFirstObjectByType<ScoringSystem>();
                    if (scoringSystem != null)
                    {
                        scoringSystem.OnCharacterSeated(database.objectsData[selectedObjectIndex], placedObject, placedObject.transform.position);
                        break;
                    }
                }
            }
        }
    }

    private void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualisationTop.SetActive(false);
        gridVisualisationBottom.SetActive(false);
        cellIndicator.SetActive(false);

        if (previewObject != null && previewObject.tag != "CannotDestroy")
        {
            Destroy(previewObject);
        }
        previewObject = emptyPreviewObject;
        previewObject.SetActive(false);

        rotation = 0;
        objectOffset = new Vector3Int(0, 0, 0);

        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        inputManager.OnRotate -= RotateStructure;
        
        originalNPC = null;
    }

    private void Update()
    {
        if (selectedObjectIndex < 0)
            return;

        Vector3 mousePosition = inputManager.GetMousePositionOnGrid();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = gridData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size, rotation);
        cellIndicatorRenderer.material.color = placementValidity ? Color.white : Color.red;

        bool isOverChair = CheckPreviewOverChair(gridPosition);
        Vector3 previewPosition = CalculateObjectPosition(gridPosition + objectOffset, isOverChair);
        previewObject.transform.position = previewPosition;

        Animator previewAnimator = previewObject.GetComponent<Animator>();
        if (previewAnimator != null)
        {
            previewAnimator.SetBool("IsSeated", isOverChair);
        }

        Material materialToChangeTo = placementValidity ? previewObjectMaterialValid : previewObjectMaterialInvalid;
        foreach (Renderer renderer in previewObjectRenderers)
        {
            renderer.material = materialToChangeTo;
        }

        Vector3 cellIndicatorPosition = CalculateObjectPosition(gridPosition + objectOffset, isOverChair);
        cellIndicatorPosition.y += 0.01f;
        cellIndicator.transform.position = cellIndicatorPosition;

        mouseIndicator.transform.position = mousePosition;
    }

    private bool CheckPreviewOverChair(Vector3Int gridPosition)
    {
        Vector3 center = grid.CellToWorld(gridPosition);
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f);

        bool isPlacingOnTop = gridPosition.y > 0;
        center.y += isPlacingOnTop ? topGridHeightOffset : bottomGridHeightOffset;

        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Chair"))
            {
                return true;
            }
        }
        return false;
    }

    private Vector3 CalculateObjectPosition(Vector3Int gridPosition, bool isOverChair)
    {
        bool isPlacingOnTop = gridPosition.y > 0;
        Vector3 position = grid.CellToWorld(gridPosition);
        position.y += isPlacingOnTop ? topGridHeightOffset : bottomGridHeightOffset;
        return position;
    }

    public void RemoveNPC(GameObject npc)
    {
        if (npc != null && peopleCounter != null && npc.CompareTag("NPC"))
        {
            peopleCounter.DecrementCount();
        }
        Destroy(npc);
    }
}