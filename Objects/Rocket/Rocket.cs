using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RocketDirection { Horizontal, Vertical }

public class Rocket : MonoBehaviour, IGridObject, IFallable, IAnimatableObject
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite horizontalRocketSprite;
    [SerializeField] private Sprite verticalRocketSprite;

    private GridManager gridManager;
    private Vector2Int gridPosition;
    private RocketDirection direction;
    private bool isAnimating = false;

    public bool IsAnimating => isAnimating;

    public void Initialize(GridManager gridManager, Vector2Int position)
    {
        this.gridManager = gridManager;
        this.gridPosition = position;

        // Randomly choose a direction
        direction = Random.Range(0, 2) == 0 ? RocketDirection.Horizontal : RocketDirection.Vertical;

        // Set the appropriate sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = direction == RocketDirection.Horizontal ?
                horizontalRocketSprite : verticalRocketSprite;
        }
    }

    public void DoWork() { }

    public void HandleFalling() { }

    void OnMouseDown()
    {
        if (gridManager == null || isAnimating) return;

        // Check for adjacent rockets to create combo
        List<Vector2Int> adjacentRockets = FindAdjacentRockets();

        if (adjacentRockets.Count > 0)
        {
            // Create a combo explosion
            CreateComboExplosion(adjacentRockets);
        }
        else
        {
            // Regular rocket explosion
            Explode();
        }
    }

    private List<Vector2Int> FindAdjacentRockets()
    {
        List<Vector2Int> rocketPositions = new List<Vector2Int>();
        Dictionary<Vector2Int, IGridObject> gridState = gridManager.getGridState();

        // Check all four adjacent positions
        CheckPositionForRocket(gridState, new Vector2Int(gridPosition.x + 1, gridPosition.y), rocketPositions);
        CheckPositionForRocket(gridState, new Vector2Int(gridPosition.x - 1, gridPosition.y), rocketPositions);
        CheckPositionForRocket(gridState, new Vector2Int(gridPosition.x, gridPosition.y + 1), rocketPositions);
        CheckPositionForRocket(gridState, new Vector2Int(gridPosition.x, gridPosition.y - 1), rocketPositions);

        return rocketPositions;
    }

    private void CheckPositionForRocket(Dictionary<Vector2Int, IGridObject> gridState, Vector2Int pos, List<Vector2Int> rockets)
    {
        if (pos.x >= 0 && pos.x < gridManager.GetGridWidth() &&
            pos.y >= 0 && pos.y < gridManager.GetGridHeight())
        {
            if (gridState.TryGetValue(pos, out IGridObject obj) && obj is Rocket)
            {
                rockets.Add(pos);
            }
        }
    }

    private void CreateComboExplosion(List<Vector2Int> adjacentRockets)
    {
        // Remove all involved rockets including this one
        foreach (Vector2Int rocketPos in adjacentRockets)
        {
            gridManager.RemoveRocket(rocketPos);
        }

        // Remove this rocket
        gridManager.RemoveRocket(gridPosition);

        // Create a 3x3 explosion around the original rocket
        StartCoroutine(ExplodeArea(gridPosition, 1));
    }

    public void Explode()
    {
        // Remove the rocket from the grid first
        gridManager.RemoveRocket(gridPosition);

        // Launch explosion parts in both directions
        if (direction == RocketDirection.Horizontal)
        {
            StartCoroutine(LaunchHorizontalExplosion());
        }
        else
        {
            StartCoroutine(LaunchVerticalExplosion());
        }
    }

    private IEnumerator LaunchHorizontalExplosion()
    {
        // Create two moving parts, one going left, one going right
        StartCoroutine(ExplodeDirection(Vector2Int.left));
        StartCoroutine(ExplodeDirection(Vector2Int.right));

        yield return null;
    }

    private IEnumerator LaunchVerticalExplosion()
    {
        // Create two moving parts, one going up, one going down
        StartCoroutine(ExplodeDirection(Vector2Int.up));
        StartCoroutine(ExplodeDirection(Vector2Int.down));

        yield return null;
    }

    private IEnumerator ExplodeDirection(Vector2Int direction)
    {
        Vector2Int currentPos = gridPosition;
        bool continuePath = true;
        float explosionDelay = 0.05f; // Slight delay between consecutive explosions

        while (continuePath)
        {
            // Move to next position
            currentPos += direction;

            // Check if position is valid
            if (currentPos.x < 0 || currentPos.x >= gridManager.GetGridWidth() ||
                currentPos.y < 0 || currentPos.y >= gridManager.GetGridHeight())
            {
                continuePath = false;
                continue;
            }

            // Get object at this position
            Dictionary<Vector2Int, IGridObject> gridState = gridManager.getGridState();
            if (!gridState.TryGetValue(currentPos, out IGridObject obj))
            {
                // Empty cell, continue path
                yield return new WaitForSeconds(explosionDelay);
                continue;
            }

            // Handle different grid objects
            if (obj is CubeObject1)
            {
                // Destroy cube
                gridManager.RemoveCube(currentPos);
                // Visual effect here

                yield return new WaitForSeconds(explosionDelay);
            }
            else if (obj is Rocket)
            {
                // Trigger rocket explosion
                Rocket rocket = ((MonoBehaviour)obj).GetComponent<Rocket>();
                if (rocket != null)
                {
                    rocket.Explode();
                }

                // Stop current path
                continuePath = false;
            }
            else if (obj is IDamageable damageable)
            {
                // Apply damage to obstacle
                if (damageable.CanTakeDamage(DamageType.Rocket))
                {
                    damageable.TakeDamage(DamageType.Rocket, 1);
                }

                // Stop if obstacle blocks path (like stone)
                if (obj is Obstacle obstacle && !obstacle.CanFall)
                {
                    continuePath = false;
                }

                yield return new WaitForSeconds(explosionDelay);
            }
            else
            {
                // Unknown object, continue path
                yield return new WaitForSeconds(explosionDelay);
            }
        }
    }

    private IEnumerator ExplodeArea(Vector2Int center, int radius)
    {
        for (int x = center.x - radius; x <= center.x + radius; x++)
        {
            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Check if position is within grid bounds
                if (x < 0 || x >= gridManager.GetGridWidth() ||
                    y < 0 || y >= gridManager.GetGridHeight())
                {
                    continue;
                }

                // Apply explosion effect to this position
                Dictionary<Vector2Int, IGridObject> gridState = gridManager.getGridState();
                if (gridState.TryGetValue(pos, out IGridObject obj) && obj != null)
                {
                    if (obj is CubeObject1)
                    {
                        gridManager.RemoveCube(pos);
                    }
                    else if (obj is IDamageable damageable)
                    {
                        if (damageable.CanTakeDamage(DamageType.Rocket))
                        {
                            damageable.TakeDamage(DamageType.Rocket, 1);
                        }
                    }
                    else if (obj is Rocket && pos != gridPosition)
                    {
                        // Trigger explosion of other rockets
                        Rocket rocket = ((MonoBehaviour)obj).GetComponent<Rocket>();
                        if (rocket != null)
                        {
                            rocket.Explode();
                        }
                    }
                }

                yield return new WaitForSeconds(0.02f); // Very slight delay for visual effect
            }
        }
    }

    public void AnimateMove(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        isAnimating = true;
        StartCoroutine(MoveCoroutine(startPosition, endPosition, duration));
    }

    private IEnumerator MoveCoroutine(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        isAnimating = false;
    }
}