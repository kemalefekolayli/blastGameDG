using UnityEngine;
using System.Collections.Generic;

public class CubeObjectFactory : MonoBehaviour, ObjectFactory<CubeObject1>
{
    public GameObject cubePrefab;

    [Header("Cube Sprites")]
    public Sprite redCubeSprite;
    public Sprite greenCubeSprite;
    public Sprite blueCubeSprite;
    public Sprite yellowCubeSprite;

    private Dictionary<string, string> colorMap;
    private Dictionary<string, Sprite> spriteMap;

    void Awake()
    {
        // Initialize color mapping
        colorMap = new Dictionary<string, string>()
        {
            { "r", "Red" },
            { "g", "Green" },
            { "b", "Blue" },
            { "y", "Yellow" }
        };

        // Initialize sprite mapping
        spriteMap = new Dictionary<string, Sprite>()
        {
            { "Red", redCubeSprite },
            { "Green", greenCubeSprite },
            { "Blue", blueCubeSprite },
            { "Yellow", yellowCubeSprite }
        };
    }

    public IGridObject CreateObject(Vector2 location, Transform parent, float cellSize, GridManager gridManager, Vector2Int gridPos)
    {
        if (cubePrefab == null)
        {
            Debug.LogError("CubeObjectFactory: No prefab assigned!");
            return null;
        }

        // Get the cube type from the matrix
        string cubeType = gridManager.GetCubeMatrix()[gridPos.x, gridPos.y];

        GameObject newCube = Instantiate(cubePrefab, new Vector3(location.x, location.y, 0), Quaternion.identity, parent);
        newCube.transform.localScale = Vector3.one * (cellSize * 0.7f);

        CubeObject1 cubeObject = newCube.GetComponent<CubeObject1>();
        if (cubeObject != null)
        {
            cubeObject.Initialize(gridManager, gridPos);

            // Set the color based on cube type
            if (colorMap.TryGetValue(cubeType, out string colorName))
            {
                cubeObject.SetColor(colorName);

                // Set the corresponding sprite
                if (spriteMap.TryGetValue(colorName, out Sprite sprite))
                {
                    cubeObject.SetSprite(sprite);
                }
            }
            else if (cubeType == "rand")
            {
                // Random color selection
                string[] colorNames = { "Red", "Green", "Blue", "Yellow" };
                string randomColor = colorNames[Random.Range(0, colorNames.Length)];
                cubeObject.SetColor(randomColor);
                cubeObject.SetSprite(spriteMap[randomColor]);
            }
            
            Debug.Log($"Created a {cubeType} cube at {location}");
            return cubeObject;
        }

        Debug.LogError("CubeObjectFactory: Created object does not implement IGridObject!");
        return null;
    }
}