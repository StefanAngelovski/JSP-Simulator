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

    [SerializeField]
    private GameObject bus; 


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

     private GameObject originalNPC; // Add this field to store the original NPC reference

    public void StartPlacement(GameObject npc)
    {
        StopPlacement();

        ObjectData npcData = database.objectsData.Find(data => data.Prefab == npc);
        if (npcData == null)
        {
            return;
        }

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == npcData.ID);
        
        // Store the original NPC reference without modifying it
        originalNPC = npc;
        
        gridVisualisationTop.SetActive(true);
        gridVisualisationBottom.SetActive(true);
        cellIndicator.SetActive(true);

        // Create preview object as a clone
        previewObject = Instantiate(npc);
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

        // Use the original NPC for placement and destroy it after setting up the new position
        GameObject newObject;
        if (originalNPC != null)
        {
            // Create the new instance first
            newObject = Instantiate(originalNPC);
            
            // Destroy the NavMeshAgent if it exists
            NavMeshAgent navMeshAgent = newObject.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                Destroy(navMeshAgent);
            }

            // Now destroy the original NPC since placement was successful
            Destroy(originalNPC);
        }
        else
        {
            // Fallback to original behavior if no originalNPC exists
            newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
            NavMeshAgent agent = newObject.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                Destroy(agent);
            }
        }

        // Set the position and rotation
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

        // Clear the originalNPC reference
        originalNPC = null;
        
        StopPlacement();
    }



    private void CheckAndSetSitting(GameObject placedObject, Vector3Int gridPosition)
    {
        // Calculate the center position based on the grid level (top or bottom)
        Vector3 center = grid.CellToWorld(gridPosition);
        bool isPlacingOnTop = gridPosition.y > 0; // Example condition for placing on the top grid

        // Adjust the Y position of the center based on the grid level
        center.y += isPlacingOnTop ? topGridHeightOffset : bottomGridHeightOffset;
        
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f);

        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Chair"))
            {
                Vector3 adjustedPosition = grid.CellToWorld(gridPosition);
                adjustedPosition.y = placedObject.transform.position.y; // Keep the Y position as it is

                placedObject.transform.position = adjustedPosition;

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
        // Don't do anything with originalNPC here - let it remain as is if placement wasn't completed
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
        
        // Clear the originalNPC reference if we're stopping placement
        originalNPC = null;
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

        // Update preview position with offsets if applicable
        Vector3 previewPosition = CalculateObjectPosition(gridPosition, isOverChair);
        previewObject.transform.position = previewPosition;

        // Update preview animator for seated state
        Animator previewAnimator = previewObject.GetComponent<Animator>();
        if (previewAnimator != null)
        {
            previewAnimator.SetBool("IsSeated", isOverChair);
        }

        // Update preview materials based on placement validity
        Material materialToChangeTo = placementValidity ? previewObjectMaterialValid : previewObjectMaterialInvalid;
        foreach (Renderer renderer in previewObjectRenderers)
        {
            renderer.material = materialToChangeTo;
        }

        // Apply the exact grid offset and placement height to the cellIndicator as well
        Vector3 cellIndicatorPosition = CalculateObjectPosition(gridPosition, isOverChair);
        cellIndicatorPosition.y += 0.01f; // Add slight offset to prevent Z-fighting
        cellIndicator.transform.position = cellIndicatorPosition;

        // Set mouse indicator to mouse position
        mouseIndicator.transform.position = mousePosition;

        // Update all placed NPCs to move with the bus
        UpdatePlacedNPCs();
    }


    private void UpdatePlacedNPCs()
    {
        foreach (GameObject placedNPC in placedGameObjects)
        {
            Vector3 busPosition = bus.transform.position;
            
            Vector3 offset = placedNPC.transform.position - grid.transform.position; // Offset relative to grid
            placedNPC.transform.position = busPosition + offset;
        }
    }



    private bool CheckPreviewOverChair(Vector3Int gridPosition)
    {
        Vector3 center = grid.CellToWorld(gridPosition); 
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f); 

        // Check if the object is on the top grid or bottom grid
        bool isPlacingOnTop = gridPosition.y > 0; // Example condition for placing on the top grid

        // Apply appropriate offset based on grid level
        if (isPlacingOnTop)
        {
            center.y += topGridHeightOffset;
        }
        else
        {
            center.y += bottomGridHeightOffset;
        }

        // Check for nearby chairs within the specified bounds
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Chair"))
            {
                return true; // Chair found nearby
            }
        }

        return false; // No chair was found in the area
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