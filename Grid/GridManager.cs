using UnityEngine;
using System.Collections.Generic;


public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; } // ✅ Singleton
    private Transform gridParent;
    public GameObject cubePrefab;
    public int gridWidth;
    public int gridHeight;
    private float cellSize;
    private Vector2 gridStartPos;
    private string[,] cubeMatrix;
    public CubeObjectFactory cubeObjectFactory;
    private Dictionary<Vector2Int, IGridObject> gridState = new Dictionary<Vector2Int, IGridObject>();
    public ObstacleFactory obstacleFactory;
    public CubeFallingHandler fallHandler;

    [Header("Rocket Setup")]
    public GameObject rocketPrefab;
    public Sprite horizontalRocketSprite;
    public Sprite verticalRocketSprite;

    // Add to GridManager.cs
    public List<Vector2Int> FindMatchingGroup(Vector2Int startPos)
    {
        List<Vector2Int> matchingGroup = new List<Vector2Int>();
        List<Vector2Int> toCheck = new List<Vector2Int>();
        HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();

        // Get the color of the starting cube
        if (!gridState.TryGetValue(startPos, out IGridObject startObject) || !(startObject is CubeObject1 cube))
            return matchingGroup;

        ObjectColor targetColor = cube.getColor();
        toCheck.Add(startPos);

        while (toCheck.Count > 0)
        {
            Vector2Int current = toCheck[0];
            toCheck.RemoveAt(0);

            if (checkedPositions.Contains(current)) continue;
            checkedPositions.Add(current);

            // Check if this is a matching cube
            if (gridState.TryGetValue(current, out IGridObject obj) &&
                obj is CubeObject1 currentCube &&
                currentCube.getColor() == targetColor)
            {
                matchingGroup.Add(current);

                // Add adjacent positions to check
                AddPositionIfValid(toCheck, new Vector2Int(current.x + 1, current.y), checkedPositions);
                AddPositionIfValid(toCheck, new Vector2Int(current.x - 1, current.y), checkedPositions);
                AddPositionIfValid(toCheck, new Vector2Int(current.x, current.y + 1), checkedPositions);
                AddPositionIfValid(toCheck, new Vector2Int(current.x, current.y - 1), checkedPositions);
            }
        }

        return matchingGroup;
    }

    private void AddPositionIfValid(List<Vector2Int> list, Vector2Int pos, HashSet<Vector2Int> checkedPositions)
    {
        if (pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight && !checkedPositions.Contains(pos))
        {
            list.Add(pos);
        }
    }

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
    // large update
private void CreateObstacleAtPosition(Vector2Int gridPos, string objectType)
    {
        Vector2 cellPosition = new Vector2(
            gridStartPos.x + (gridPos.x * cellSize),
            gridStartPos.y + (gridPos.y * cellSize)
        );

        // Create obstacle with factory
        IGridObject obstacle = obstacleFactory.CreateObstacle(
            objectType, cellPosition, gridParent, cellSize, this, gridPos);

        if (obstacle != null)
        {
            gridState[gridPos] = obstacle;
            Debug.Log($"Created obstacle at {gridPos} with type {objectType}");
        }
    }

    // Check for obstacle types in SyncGridStateWithMatrix
    private bool IsObstacleType(string objectType)
    {
        return objectType == "bo" || objectType == "s" || objectType == "v";
    }

    // Method to handle obstacle removal
    public void RemoveObstacle(Vector2Int gridPosition)
    {
        if (gridState.ContainsKey(gridPosition) && gridState[gridPosition] != null)
        {
            MonoBehaviour obj = gridState[gridPosition] as MonoBehaviour;
            if (obj != null)
            {
                Destroy(obj.gameObject);
                Debug.Log($"Obstacle removed at {gridPosition}");
            }

            // Mark position as empty
            cubeMatrix[gridPosition.x, gridPosition.y] = "empty";
            gridState[gridPosition] = null;

            // Handle falling objects if needed
            if (fallHandler != null)
            {
                fallHandler.HandleFalling();
            }
        }
    }

    // Method to damage obstacles after a cube blast
    public void DamageAdjacentObstacles(List<Vector2Int> blastPositions)
    {
        HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();

        foreach (Vector2Int blastPos in blastPositions)
        {
            // Check all adjacent positions
            CheckAndDamageObstacle(new Vector2Int(blastPos.x + 1, blastPos.y), DamageType.Adjacent, processedPositions);
            CheckAndDamageObstacle(new Vector2Int(blastPos.x - 1, blastPos.y), DamageType.Adjacent, processedPositions);
            CheckAndDamageObstacle(new Vector2Int(blastPos.x, blastPos.y + 1), DamageType.Adjacent, processedPositions);
            CheckAndDamageObstacle(new Vector2Int(blastPos.x, blastPos.y - 1), DamageType.Adjacent, processedPositions);
        }
    }

    private void CheckAndDamageObstacle(Vector2Int pos, DamageType damageType, HashSet<Vector2Int> processedPositions)
    {
        // Check if position is valid and hasn't been processed yet
        if (pos.x < 0 || pos.x >= gridWidth || pos.y < 0 || pos.y >= gridHeight ||
            processedPositions.Contains(pos))
        {
            return;
        }

        processedPositions.Add(pos);

        // Check if there's an obstacle at this position
        if (gridState.TryGetValue(pos, out IGridObject obj) && obj != null && obj is IDamageable damageable)
        {
            // Apply damage if the obstacle can take this type of damage
            if (damageable.CanTakeDamage(damageType))
            {
                damageable.TakeDamage(damageType, 1);
            }
        }
    }

    // Method to update existing FindMatchingGroup method
    // This updated method includes a check for "isRocketEligible"
    public List<Vector2Int> FindMatchingGroup(Vector2Int startPos, out bool isRocketEligible)
    {
        List<Vector2Int> matchingGroup = new List<Vector2Int>();
        List<Vector2Int> toCheck = new List<Vector2Int>();
        HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();
        isRocketEligible = false;

        // Get the color of the starting cube
        if (!gridState.TryGetValue(startPos, out IGridObject startObject) || !(startObject is CubeObject1 cube))
            return matchingGroup;

        ObjectColor targetColor = cube.getColor();
        toCheck.Add(startPos);

        while (toCheck.Count > 0)
        {
            Vector2Int current = toCheck[0];
            toCheck.RemoveAt(0);

            if (checkedPositions.Contains(current)) continue;
            checkedPositions.Add(current);

            // Check if this is a matching cube
            if (gridState.TryGetValue(current, out IGridObject obj) &&
                obj is CubeObject1 currentCube &&
                currentCube.getColor() == targetColor)
            {
                matchingGroup.Add(current);

                // Add adjacent positions to check
                AddPositionIfValid(toCheck, new Vector2Int(current.x + 1, current.y), checkedPositions);
                AddPositionIfValid(toCheck, new Vector2Int(current.x - 1, current.y), checkedPositions);
                AddPositionIfValid(toCheck, new Vector2Int(current.x, current.y + 1), checkedPositions);
                AddPositionIfValid(toCheck, new Vector2Int(current.x, current.y - 1), checkedPositions);
            }
        }

        // Set rocket eligibility if group size >= 4
        isRocketEligible = matchingGroup.Count >= 4;

        return matchingGroup;
    }

    public void CreateRocket(Vector2Int position)
        {
            if (rocketPrefab == null)
            {
                Debug.LogError("Rocket prefab is not assigned!");
                return;
            }

            // Calculate world position
            Vector2 worldPos = new Vector2(
                gridStartPos.x + (position.x * cellSize),
                gridStartPos.y + (position.y * cellSize)
            );

            // Remove the existing cube first
            RemoveCube(position);

            // Create the rocket
            GameObject rocketObj = Instantiate(rocketPrefab, new Vector3(worldPos.x, worldPos.y, 0), Quaternion.identity, gridParent);
            rocketObj.transform.localScale = Vector3.one * (cellSize * 0.8f);

            // Initialize the rocket
            Rocket rocket = rocketObj.GetComponent<Rocket>();
            if (rocket != null)
            {
                rocket.Initialize(this, position);

                // Set the sprites
                SpriteRenderer sr = rocketObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Assign sprites (these will be used based on direction)
                    Rocket rocketComponent = rocketObj.GetComponent<Rocket>();
                    if (rocketComponent != null)
                    {
                        // Use reflection to set private serialized fields
                        System.Reflection.FieldInfo horizontalField = typeof(Rocket).GetField("horizontalRocketSprite",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                        System.Reflection.FieldInfo verticalField = typeof(Rocket).GetField("verticalRocketSprite",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                        if (horizontalField != null && verticalField != null)
                        {
                            horizontalField.SetValue(rocketComponent, horizontalRocketSprite);
                            verticalField.SetValue(rocketComponent, verticalRocketSprite);
                        }
                    }
                }

                // Update the grid state and matrix
                gridState[position] = rocket;

                // Mark the cell as a rocket in the cube matrix (use 'hro' or 'vro' based on direction)
                // Direction is randomly set in Rocket.Initialize, so we can't determine it here
                // Use a generic code for now
                cubeMatrix[position.x, position.y] = "ro"; // Generic rocket code

                Debug.Log($"Created rocket at {position}");
            }
            else
            {
                Debug.LogError("Rocket component not found on prefab!");
                Destroy(rocketObj);
            }
        }

        // Remove a rocket from the grid
        public void RemoveRocket(Vector2Int position)
        {
            if (gridState.TryGetValue(position, out IGridObject obj) && obj is Rocket)
            {
                MonoBehaviour rocketObj = obj as MonoBehaviour;
                if (rocketObj != null)
                {
                    // Don't actually destroy the GameObject yet if it's part of an animation
                    // Just mark it for removal from the grid
                    Destroy(rocketObj.gameObject);
                }

                // Update the grid state and matrix
                gridState[position] = null;
                cubeMatrix[position.x, position.y] = "empty";
            }
        }

        // Check if a group is eligible to create a rocket and display a rocket hint
        public void CheckForRocketHint(List<Vector2Int> group)
        {
            if (group.Count >= 4)
            {
                // Visual indicator for rocket creation potential
                foreach (Vector2Int pos in group)
                {
                    if (gridState.TryGetValue(pos, out IGridObject obj) && obj is CubeObject1 cube)
                    {
                        // Call a method on the cube to show rocket hint
                        // This could add a rocket icon overlay to the cube
                        cube.ShowRocketHint(true);
                    }
                }
            }
            else
            {
                // Clear any existing hints
                foreach (Vector2Int pos in group)
                {
                    if (gridState.TryGetValue(pos, out IGridObject obj) && obj is CubeObject1 cube)
                    {
                        cube.ShowRocketHint(false);
                    }
                }
            }
        }

    public float GetCellSize() => cellSize;
    public int GetGridHeight() => gridHeight;
    public int GetGridWidth() => gridWidth;
    public string[,] GetCubeMatrix() => cubeMatrix;
    public Dictionary<Vector2Int, IGridObject> getGridState() => gridState;
    public Vector2 GetGridStartPos() => gridStartPos;
}
