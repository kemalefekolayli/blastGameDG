using UnityEngine;
using System.Reflection;

public class ObstacleFactory : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    public GameObject boxPrefab;
    public GameObject stonePrefab;
    public GameObject vasePrefab;

    [Header("Obstacle Sprites")]
    public Sprite boxSprite;
    public Sprite stoneSprite;
    public Sprite vaseSprite;
    public Sprite damagedVaseSprite;

    public IGridObject CreateObstacle(string obstacleType, Vector2 location, Transform parent, float cellSize, GridManager gridManager, Vector2Int gridPos)
    {
        GameObject prefab = null;

        switch (obstacleType)
        {
            case "bo": // Box
                prefab = boxPrefab;
                break;
            case "s": // Stone
                prefab = stonePrefab;
                break;
            case "v": // Vase
                prefab = vasePrefab;
                break;
            default:
                Debug.LogError($"Unknown obstacle type: {obstacleType}");
                return null;
        }

        if (prefab == null)
        {
            Debug.LogError($"No prefab assigned for obstacle type: {obstacleType}");
            return null;
        }

        // Instantiate the obstacle
        GameObject obstacleObj = Instantiate(prefab, new Vector3(location.x, location.y, 0), Quaternion.identity, parent);
        obstacleObj.transform.localScale = Vector3.one * (cellSize * 0.8f);

        // Set up the obstacle
        IGridObject obstacle = null;

        if (obstacleType == "bo")
        {
            BoxObstacle box = obstacleObj.GetComponent<BoxObstacle>();
            if (box != null)
            {
                box.Initialize(gridManager, gridPos);
                SpriteRenderer sr = box.GetComponent<SpriteRenderer>();
                if (sr != null && boxSprite != null)
                {
                    sr.sprite = boxSprite;
                }
                obstacle = box;
            }
        }
        else if (obstacleType == "s")
        {
            StoneObstacle stone = obstacleObj.GetComponent<StoneObstacle>();
            if (stone != null)
            {
                stone.Initialize(gridManager, gridPos);
                SpriteRenderer sr = stone.GetComponent<SpriteRenderer>();
                if (sr != null && stoneSprite != null)
                {
                    sr.sprite = stoneSprite;
                }
                obstacle = stone;
            }
        }
        else if (obstacleType == "v")
        {
            VaseObstacle vase = obstacleObj.GetComponent<VaseObstacle>();
            if (vase != null)
            {
                vase.Initialize(gridManager, gridPos);
                SpriteRenderer sr = vase.GetComponent<SpriteRenderer>();
                if (sr != null && vaseSprite != null)
                {
                    sr.sprite = vaseSprite;
                }

                // Store the damaged sprite reference using reflection
                FieldInfo damagedSpriteField = typeof(VaseObstacle).GetField("damagedSprite",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (damagedSpriteField != null)
                {
                    damagedSpriteField.SetValue(vase, damagedVaseSprite);
                }

                obstacle = vase;
            }
        }

        if (obstacle == null)
        {
            Debug.LogError($"Failed to create obstacle of type: {obstacleType}");
            Destroy(obstacleObj);
            return null;
        }

        Debug.Log($"Created a {obstacleType} obstacle at {location}");
        return obstacle;
    }
}