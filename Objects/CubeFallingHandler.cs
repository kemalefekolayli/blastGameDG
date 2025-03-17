using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public  class CubeFallingHandler : MonoBehaviour,  IFallable
{
    public GridManager gridManager;
    private Dictionary<Vector2Int, IGridObject> gridState;
    private int gridHeight;
    private int gridWidth;
    private Vector2 startPos;

    public void HandleFalling()

    {
        Debug.Log("Handling falling...");
        gridHeight = gridManager.GetGridHeight();
        gridWidth = gridManager.GetGridWidth();

        // Process column by column
        for (int x = 0; x < gridWidth; x++)
        {
            // Bottom to top
            for (int y = 0; y < gridHeight; y++)
            {
                // If this cell is empty, we look above for a non-empty
                if (gridState.TryGetValue(new Vector2Int(x, y), out IGridObject current) && current == null)
                {
                    // Search above from y+1 to top
                    for (int aboveY = y + 1; aboveY < gridHeight; aboveY++)
                    {
                        Vector2Int abovePos = new Vector2Int(x, aboveY);

                        if (gridState.TryGetValue(abovePos, out IGridObject aboveObj) && aboveObj != null)
                        {
                            // Found a cube above. Let's move it down.
                            MoveCube(x, aboveY, x, y);
                            break; // Once one cube falls, break to re-check the same y
                        }
                    }
                }
            }
        }


        Debug.Log("Falling complete.");
    }

private void MoveCube(int fromX, int fromY, int toX, int toY) // should probably create a separete class for this but im too tired
{
    Vector2Int fromPos = new Vector2Int(fromX, fromY);
    Vector2Int toPos = new Vector2Int(toX, toY);
    Vector2 startPos = gridManager.GetGridStartPos();


    // 1) Update gridState
    IGridObject fallingCube = gridState[fromPos];
    gridState[fromPos] = null;
    gridState[toPos] = fallingCube;

    // 2) Update cubeMatrix if you’re mirroring object types
    string[,] cubeMatrix = gridManager.GetCubeMatrix();
    string oldType = cubeMatrix[fromX, fromY];
    cubeMatrix[fromX, fromY] = "empty";
    cubeMatrix[toX, toY] = oldType;

    // 3) Move the actual GameObject
    MonoBehaviour cubeScript = fallingCube as MonoBehaviour;
    if (cubeScript != null)
    {
        Vector3 newPos = new Vector3(
            startPos.x + (toX * gridManager.GetCellSize()),
            startPos.y + (toY * gridManager.GetCellSize()),
            0
        );
        cubeScript.transform.position = newPos;
    }

    Debug.Log($"Cube moved from ({fromX},{fromY}) to ({toX},{toY}).");
}


}
