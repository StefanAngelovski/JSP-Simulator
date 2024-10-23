using UnityEngine;

public class GridLimitSize : MonoBehaviour
{
    public GameObject seatPrefab;    // The 3D seat model to place
    public int gridWidth = 5;        // Number of columns in the grid
    public int gridHeight = 5;       // Number of rows in the grid
    public int layers = 2;           // Number of layers in the y-direction (height)

    [Range(0.1f, 5f)]
    public float xSpacing = 0.5f;      // Horizontal spacing between seats
    [Range(0.1f, 5f)]
    public float ySpacing = 0.5f;      // Vertical spacing between seats

    public float cellSize = 1f;       // Size of each grid cell

    private GameObject[,,] grid;      // 3D array to hold seat objects

    private int lastGridWidth;         // Store the last width for comparison
    private int lastGridHeight;        // Store the last height for comparison
    private float lastXSpacing;        // Store the last x-spacing for comparison
    private float lastYSpacing;        // Store the last y-spacing for comparison

    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight, layers];
        CreateGrid();
        lastGridWidth = gridWidth;     // Initialize last width
        lastGridHeight = gridHeight;   // Initialize last height
        lastXSpacing = xSpacing;       // Initialize last x-spacing
        lastYSpacing = ySpacing;       // Initialize last y-spacing
    }

    void Update()
    {
        // Check if grid dimensions or spacing have changed
        if (gridWidth != lastGridWidth || gridHeight != lastGridHeight ||
            xSpacing != lastXSpacing || ySpacing != lastYSpacing)
        {
            CreateGrid(); // Update the grid if dimensions or spacing changed
        }
    }

    void CreateGrid()
    {
        // Clear existing grid
        ClearGrid();

        // Resize the grid based on current gridWidth and gridHeight
        grid = new GameObject[gridWidth, gridHeight, layers];

        for (int z = 0; z < layers; z++) // Loop through layers in the y-direction
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    PlaceObject(x, y, z, seatPrefab); // Place the seat model in each tile across all layers
                }
            }
        }

        // Store the current dimensions for the next frame comparison
        lastGridWidth = gridWidth;
        lastGridHeight = gridHeight;
        lastXSpacing = xSpacing; // Update last x-spacing
        lastYSpacing = ySpacing; // Update last y-spacing
    }

    void ClearGrid()
    {
        for (int z = 0; z < layers; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y, z] != null)
                    {
                        Destroy(grid[x, y, z]); // Remove existing seats
                        grid[x, y, z] = null;    // Clear reference
                    }
                }
            }
        }
    }

    public void PlaceObject(int x, int y, int z, GameObject objectToPlace)
    {
        if (grid[x, y, z] == null) // Check if the space is free
        {
            // Calculate world position using cell size and spacing
            Vector3 worldPos = new Vector3(
                x * (cellSize + xSpacing),    // Horizontal position
                z * (cellSize + ySpacing),    // Vertical position (layer height)
                y * (cellSize + xSpacing)     // Forward position (depth)
            );

            // Adjust the rotation to spawn the chair upright
            Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f); // Rotate 90 degrees on the x-axis

            grid[x, y, z] = Instantiate(objectToPlace, worldPos, rotation); // Place the seat with adjusted rotation
        }
    }
}
