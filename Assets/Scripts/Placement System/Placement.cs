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

    // Sitting position offset, adjustable in the Unity Inspector
    [SerializeField]
    private Vector3 sittingPositionOffset = new Vector3(0, 0, 0);

    private int rotation = 0;
    private Vector3Int objectOffset;

    private void Start()
    {
        previewObject = emptyPreviewObject;

        StopPlacement();
        gridData = new();

        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
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
        StopPlacement(); // Stop any existing placement

        ObjectData npcData = database.objectsData.Find(data => data.Prefab == npc);
        if (npcData == null)
        {
            return; // NPC data not found
        }

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == npcData.ID);
        
        // Continue with the rest of your StartPlacement logic...
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

        // Instantiate the new object
        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);

        // Remove NavMeshAgent if it exists
        NavMeshAgent navMeshAgent = newObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            Destroy(navMeshAgent); 
        }

        // Apply the consistent position calculation
        Vector3 objectPosition = CalculateObjectPosition(placePosition, CheckPreviewOverChair(gridPosition));
        newObject.transform.position = objectPosition;
        newObject.transform.rotation = Quaternion.Euler(0, rotation * 90, 0);
        placedGameObjects.Add(newObject);

        gridData.AddObjectAt(gridPosition,
            rotation,
            database.objectsData[selectedObjectIndex].Size,
            database.objectsData[selectedObjectIndex].ID,
            placedGameObjects.Count - 1);

        CheckAndSetSitting(newObject, placePosition);
    }

    private void CheckAndSetSitting(GameObject placedObject, Vector3Int gridPosition)
    {
        Vector3 center = grid.CellToWorld(gridPosition);
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f); 

        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Chair"))
            {
                Vector3 gridCenter = grid.CellToWorld(gridPosition);
                gridCenter.y = placedObject.transform.position.y; // Keep current Y position

                placedObject.transform.position = gridCenter;

                Animator animator = placedObject.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetBool("IsSeated", true);
                    ApplySittingOffset(placedObject);

                    GridCollisionDetection gridCollisionDetection = FindFirstObjectByType<GridCollisionDetection>();
                    if (gridCollisionDetection != null)
                    {
                        gridCollisionDetection.OnCharacterSeated(database.objectsData[selectedObjectIndex], placedObject, placedObject.transform.position);
                        break;
                    }
                }
            }
        }
    }

    private void ApplySittingOffset(GameObject placedObject)
    {
        placedObject.transform.position += sittingPositionOffset;
    }

    private void StopPlacement()
    {
        selectedObjectIndex = -1;

        gridVisualisationTop.SetActive(false);
        gridVisualisationBottom.SetActive(false);
        cellIndicator.SetActive(false);

        if (previewObject.tag != "CannotDestroy")
            Destroy(previewObject);
        previewObject = emptyPreviewObject;
        previewObject.SetActive(false);

        rotation = 0;
        objectOffset = new Vector3Int(0, 0, 0);

        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        inputManager.OnRotate -= RotateStructure;
    }

    void Update()
    {
        if (selectedObjectIndex < 0)
            return;

        Vector3 mousePosition = inputManager.GetMousePositionOnGrid();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = gridData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size, rotation);
        cellIndicatorRenderer.material.color = placementValidity ? Color.white : Color.red;

        bool isOverChair = CheckPreviewOverChair(gridPosition);

        // Consistently apply calculated position to the preview object
        Vector3 previewPosition = CalculateObjectPosition(gridPosition, isOverChair);
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

        Vector3 cellIndicatorPosition = grid.CellToWorld(gridPosition);
        cellIndicatorPosition.y += 0.1f;
        cellIndicator.transform.position = cellIndicatorPosition;

        mouseIndicator.transform.position = mousePosition;
    }

    private bool CheckPreviewOverChair(Vector3Int gridPosition)
    {
        Vector3 center = grid.CellToWorld(gridPosition); // Get the world position of the grid cell
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f); // Set the size of the overlap box

        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Chair"))
            {
                return true; // A chair is found within the overlap box
            }
        }

        return false; // No chair was found
    }

    [SerializeField]
    private float topGridHeightOffset = 0.1f;  // Adjust as needed for top grid alignment
    [SerializeField]
    private float bottomGridHeightOffset = 5f;  // Adjust for bottom grid

    private Vector3 CalculateObjectPosition(Vector3Int gridPosition, bool isOverChair)
    {
        // Determine if we're placing on the top or bottom grid
        bool isPlacingOnTop = gridPosition.y > 0; // Example condition for placing on the top grid

        Vector3 position = grid.CellToWorld(gridPosition);

        // Adjust position based on the grid level and whether over a chair
        if (isPlacingOnTop)
        {
            position.y += topGridHeightOffset; // Offset for top grid
        }
        else
        {
            position.y += bottomGridHeightOffset; // Offset for bottom grid
        }

        // Further adjust if over a chair
        if (isOverChair)
        {
            position.y += sittingPositionOffset.y; // Adjust height when seated
        }

        return position;
    }
}
