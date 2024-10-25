using UnityEngine;

public class GridData : MonoBehaviour
{
    // Example grid size and other relevant properties
    public int gridWidth;
    public int gridHeight;
    public GameObject objectPrefab; // Prefab to instantiate
    private GameObject[,] grid; // 2D array for grid storage

    void Start()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        grid = new GameObject[gridWidth, gridHeight];
    }

    public bool TryPlaceObject(Vector3 position, out int objectID)
    {
        // Convert the position to grid coordinates
        Vector2Int gridPosition = WorldToGrid(position);
        
        // Check if the position is within the grid boundaries
        if (IsValidPosition(gridPosition))
        {
            // Check if there's already an object in the grid cell
            if (grid[gridPosition.x, gridPosition.y] == null)
            {
                // Place the object and assign its ID
                GameObject newObject = Instantiate(objectPrefab, position, Quaternion.identity);
                grid[gridPosition.x, gridPosition.y] = newObject;
                
                // Assuming we can generate an ID from the instance ID
                objectID = newObject.GetInstanceID(); // or a custom ID assignment method
                return true; // Successfully placed the object
            }
            else
            {
                // If there is already an object, set objectID to indicate failure
                objectID = -1; // Indicate failure due to existing object
                return false; // Indicate failure to place object
            }
        }
        else
        {
            // Assign a default value when the placement fails
            objectID = -1; // Default or error value
            return false; // Indicate failure to place object
        }
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth && position.y >= 0 && position.y < gridHeight;
    }

    private Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        // Convert world position to grid coordinates (assuming a simple scaling for this example)
        int x = Mathf.FloorToInt(worldPosition.x);
        int y = Mathf.FloorToInt(worldPosition.z); // Assuming a 2D plane on XZ
        return new Vector2Int(x, y);
    }

    // Optional: Add a method to clear a specific object
    public void ClearObject(Vector2Int gridPosition)
    {
        if (IsValidPosition(gridPosition) && grid[gridPosition.x, gridPosition.y] != null)
        {
            Destroy(grid[gridPosition.x, gridPosition.y]);
            grid[gridPosition.x, gridPosition.y] = null;
        }
    }
}
