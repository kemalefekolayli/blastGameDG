using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; } // ✅ Singleton
    private Transform gridParent;
    public GameObject cubePrefab;
    private int gridWidth;
    private int gridHeight;
    private float cellSize;
    private Vector2 gridStartPos;
    private string[,] cubeMatrix;
    public CubeObjectFactory cubeObjectFactory;
    private Dictionary<Vector2Int, IGridObject> gridState = new Dictionary<Vector2Int, IGridObject>();
    public CubeFallingHandler fallHandler;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ Keeps GridManager alive
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LevelData level1 = LevelLoader.Instance.GetLevel(1);
        if (level1 == null)
        {
            Debug.LogError("Level data is NULL.");
            return;
        }

        gridWidth = level1.grid_width;
        gridHeight = level1.grid_height;
        cubeMatrix = LoadGridData(gridWidth, gridHeight, level1.grid);

        Debug.Log($"Grid Width: {gridWidth}, Grid Height: {gridHeight}");
        GameObject parentObj = new GameObject("GridParent");
        gridParent = parentObj.transform;

        // ✅ Dynamic grid scaling
        float gridMaxWidth = 4f;
        float gridMaxHeight = 4f;
        cellSize = Mathf.Min(gridMaxWidth / gridWidth, gridMaxHeight / gridHeight);

        // ✅ Calculate starting position to center the grid
        Vector2 screenCenter = new Vector2(0, 0);
        gridStartPos = new Vector2(
            screenCenter.x - ((gridWidth * cellSize) / 2) + (cellSize / 2),
            screenCenter.y - ((gridHeight * cellSize) / 2) + (cellSize / 2)
        );

        SyncGridStateWithMatrix(); // ✅ Ensures objects match cubeMatrix
    }

    public string[,] LoadGridData(int width, int height, string[] gridData)
    {
        string[,] gridArray = new string[width, height];
        int index = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gridArray[x, y] = gridData[index];
                index++;
            }
        }
        return gridArray;
    }

    public void SyncGridStateWithMatrix()
    {
        Debug.Log("Syncing cubeMatrix with gridState...");

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                string objectType = cubeMatrix[x, y];

                // ✅ If the position is already occupied, skip it
                if (gridState.ContainsKey(gridPos) && gridState[gridPos] != null)
                {
                    continue;
                }

                // ✅ Ensure we have a valid object type before creating
                if (!string.IsNullOrEmpty(objectType) && objectType != "empty")
                {
                    Vector2 cellPosition = new Vector2(
                        gridStartPos.x + (x * cellSize),
                        gridStartPos.y + (y * cellSize)
                    );

                    // ✅ Create the object using the factory
                    IGridObject newObject = cubeObjectFactory.CreateObject(cellPosition, gridParent, cellSize, this, gridPos);

                    if (newObject != null)
                    {
                        gridState[gridPos] = newObject;
                        Debug.Log($"Created object at {gridPos} with type {objectType}");
                    }
                }
                else
                {
                    gridState[gridPos] = null; // ✅ Ensure "empty" slots are null in the dictionary
                }
            }
        }
        Debug.Log("Syncing complete.");
    }

    public void RemoveCube(Vector2Int gridPosition)
    {
        Debug.Log($"Attempting to remove cube at {gridPosition}");

        // Check if the cube exists in the dictionary
        if (gridState.ContainsKey(gridPosition))
        {
            IGridObject gridObject = gridState[gridPosition];

            // Ensure the object exists before removing
            if (gridObject != null)
            {
                MonoBehaviour obj = gridObject as MonoBehaviour;
                if (obj != null)
                {
                    Destroy(obj.gameObject); // ✅ Destroy the actual GameObject
                    Debug.Log($"Cube removed at {gridPosition}");
                }
            }

            // ✅ Mark position as "empty" in cubeMatrix
                cubeMatrix[gridPosition.x, gridPosition.y] = "empty";
                gridState[gridPosition] = null;

                // Call HandleFalling AFTER updating the grid state
                if (fallHandler != null)
                {
                    fallHandler.HandleFalling();
                }
                else
                {
                    Debug.LogError("FallHandler is null!");
                }
        }
        else
        {
            Debug.LogWarning($"No cube found at {gridPosition} to remove.");
        }
    }

    public float GetCellSize() => cellSize;
    public int GetGridHeight() => gridHeight;
    public int GetGridWidth() => gridWidth;
    public string[,] GetCubeMatrix() => cubeMatrix;
    public Dictionary<Vector2Int, IGridObject> getGridState() => gridState;
    public Vector2 GetGridStartPos() => gridStartPos;
}
