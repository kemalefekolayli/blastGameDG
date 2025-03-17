using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CubeFallingHandler : MonoBehaviour, IFallable
{
    public GridManager gridManager;
    private Dictionary<Vector2Int, IGridObject> gridState;
    private int gridHeight;
    private int gridWidth;
    private Vector2 startPos;

    [SerializeField] private float fallDuration = 0.3f;
    private bool isAnimating = false;

    // Queue for pending animations
    private Queue<(Vector2Int from, Vector2Int to)> pendingMoves = new Queue<(Vector2Int, Vector2Int)>();

    public void HandleFalling()
    {
        if (isAnimating) return;

        Debug.Log("Handling falling...");
        gridHeight = gridManager.GetGridHeight();
        gridWidth = gridManager.GetGridWidth();
        gridState = gridManager.getGridState();

        // Clear previous pending moves
        pendingMoves.Clear();

        // Find all moves that need to happen
        for (int x = 0; x < gridWidth; x++)
        {
            ProcessColumn(x);
        }

        // Start the animation sequence
        if (pendingMoves.Count > 0)
        {
            StartCoroutine(AnimateFallingSequence());
        }
    }

    private void ProcessColumn(int x)
    {
        // Start from bottom of grid (y=0) and work up
        int firstEmptyY = -1;

        for (int y = 0; y < gridHeight; y++)
        {
            Vector2Int pos = new Vector2Int(x, y);

            if (!gridState.TryGetValue(pos, out IGridObject obj) || obj == null)
            {
                // Found empty cell
                if (firstEmptyY == -1) firstEmptyY = y;
            }
            else if (firstEmptyY != -1)
            {
                // Found object above empty space - queue it to fall
                pendingMoves.Enqueue((pos, new Vector2Int(x, firstEmptyY)));

                // Update grid state
                gridState[new Vector2Int(x, firstEmptyY)] = obj;
                gridState[pos] = null;

                // Update cube matrix
                string[,] cubeMatrix = gridManager.GetCubeMatrix();
                cubeMatrix[x, firstEmptyY] = cubeMatrix[x, y];
                cubeMatrix[x, y] = "empty";

                // Update empty position for next object
                firstEmptyY++;
            }
        }
    }

    private IEnumerator AnimateFallingSequence()
    {
        isAnimating = true;
        startPos = gridManager.GetGridStartPos(); // Get current startPos

        while (pendingMoves.Count > 0)
        {
            var move = pendingMoves.Dequeue();

            // Calculate world positions correctly
            Vector3 fromWorldPos = new Vector3(
                startPos.x + (move.from.x * gridManager.GetCellSize()),
                startPos.y + (move.from.y * gridManager.GetCellSize()),
                0
            );

            Vector3 toWorldPos = new Vector3(
                startPos.x + (move.to.x * gridManager.GetCellSize()),
                startPos.y + (move.to.y * gridManager.GetCellSize()),
                0
            );

            // Get object and animate
            IGridObject obj = gridState[move.to];
            if (obj is MonoBehaviour monoBehaviour && obj is IAnimatableObject animObj)
            {
                animObj.AnimateMove(fromWorldPos, toWorldPos, fallDuration);

                // Wait for animation to complete
                yield return new WaitForSeconds(fallDuration);
            }
        }

        isAnimating = false;
    }
}
