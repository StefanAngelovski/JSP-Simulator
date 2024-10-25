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

    private float placementHeightOffset = 0.1f;

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
            if (selectedObjectIndex != -1)
                Debug.LogError($"No ID found, {ID}");
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
        if (inputManager.IsPointerOverUI())
            return;

        Vector3 mousePosition = inputManager.GetMousePositionOnGrid();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        Vector3Int placePosition = gridPosition + objectOffset;

        bool placementValidity = gridData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size, rotation);

        if (!placementValidity)
        {
            Debug.Log("Invalid Pos");
            return;
        }

        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);

        Vector3 objectPosition = grid.CellToWorld(placePosition);
        objectPosition.y += placementHeightOffset;

        newObject.transform.position = objectPosition;
        newObject.transform.rotation = Quaternion.Euler(0, rotation * 90, 0);
        placedGameObjects.Add(newObject);

        gridData.AddObjectAt(gridPosition,
            rotation,
            database.objectsData[selectedObjectIndex].Size,
            database.objectsData[selectedObjectIndex].ID,
            placedGameObjects.Count - 1);

        // Remove NavMeshAgent component if it exists
        NavMeshAgent navMeshAgent = newObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            Destroy(navMeshAgent);  // Programmatically remove the NavMeshAgent
        }

        // Check if the object is placed over a chair and update its Animator
        CheckAndSetSitting(newObject, placePosition);
    }


    private void CheckAndSetSitting(GameObject placedObject, Vector3Int gridPosition)
    {
        // Create a box to check for chairs in the vicinity of the placed object.
        Vector3 center = grid.CellToWorld(gridPosition);
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f);  // Adjust size based on your grid cell size and object size

        // Assume chairs have a tag "Chair"
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Chair"))
            {
              //  Debug.Log("Chair detected underneath!");

                // Center the object on the grid
                Vector3 gridCenter = grid.CellToWorld(gridPosition);
                gridCenter.y = placedObject.transform.position.y; // Keep current Y position

                placedObject.transform.position = gridCenter;

                // Find the Animator in the placed object and set IsSeated to true
                Animator animator = placedObject.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetBool("IsSeated", true);
                   // Debug.Log("Character is sitting!");

                    // Apply the sitting position offset (which can be adjusted in real time)
                    ApplySittingOffset(placedObject);

                    
                    GridCollisionDetection gridCollisionDetection = FindObjectOfType<GridCollisionDetection>();
                        if (gridCollisionDetection != null)
                        {
                            gridCollisionDetection.OnCharacterSeated(database.objectsData[selectedObjectIndex], placedObject.transform.position);
                            break;
                        }



                }
            }
        }
    }

    // Method to apply the sitting offset, can be adjusted real-time via the Inspector
    private void ApplySittingOffset(GameObject placedObject)
    {
        placedObject.transform.position += sittingPositionOffset;
    }

    private void StopPlacement()
    {
        selectedObjectIndex = -1;

        // Deactivate both grid visualizations
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

        Vector3 previewPosition = grid.CellToWorld(gridPosition + objectOffset);
        previewPosition.y += placementHeightOffset;
        previewObject.transform.position = previewPosition;

        Material materialToChangeTo = placementValidity ? previewObjectMaterialValid : previewObjectMaterialInvalid;
        foreach (Renderer renderer in previewObjectRenderers)
        {
            renderer.material = materialToChangeTo;
        }

        // Adjust cellIndicator position slightly above the grid
        Vector3 cellIndicatorPosition = grid.CellToWorld(gridPosition);
        cellIndicatorPosition.y += 0.1f; // Raise it by 0.2 units on the Y-axis
        cellIndicator.transform.position = cellIndicatorPosition;

        mouseIndicator.transform.position = mousePosition;
    }


    // Optional: To apply offset in real-time while the game is running
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            // Update the sitting position offset in real-time if changes occur in the inspector
            foreach (var placedObject in placedGameObjects)
            {
                if (placedObject != null && placedObject.GetComponent<Animator>()?.GetBool("IsSeated") == true)
                {
                    ApplySittingOffset(placedObject);
                }
            }
        }
    }
}
