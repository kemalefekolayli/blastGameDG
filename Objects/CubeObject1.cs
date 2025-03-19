using UnityEngine;
using System.Collections;

public enum ObjectColor { Red, Green, Blue, Yellow }

public class CubeObject1 : MonoBehaviour, IGridObject, IFallable, IAnimatableObject
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private ObjectColor cubeColor;
    private GridManager gridManager;
    private Vector2Int gridPosition;
    private bool isAnimating = false;
    public bool IsAnimating => isAnimating;

    public void DoWork(){
        Debug.Log("zort");
    }

    public void Initialize(GridManager gridManager, Vector2Int position)
    {
        this.gridManager = gridManager;
        this.gridPosition = position;
        SetColor(gridManager.GetCubeMatrix()[position.x, position.y]);
    }

    void Start()
    {
    }

    void OnMouseDown()
        {
            if (gridManager == null || isAnimating) return;

            bool isRocketEligible;
            var matchingGroup = gridManager.FindMatchingGroup(gridPosition, out isRocketEligible);

            if (matchingGroup.Count >= 2)
            {
                // Create a rocket if eligible (4+ matching cubes)
                if (isRocketEligible)
                {
                    gridManager.CreateRocket(gridPosition);
                }
                else
                {
                    // Process normal blast
                    foreach (var pos in matchingGroup)
                    {
                        gridManager.RemoveCube(pos);
                    }

                    // Damage adjacent obstacles
                    gridManager.DamageAdjacentObstacles(matchingGroup);
                }
            }
        }

    public void HandleFalling(){
    }

    public void AnimateMove(Vector3 startPosition, Vector3 endPosition, float duration){
        isAnimating = true;
        StartCoroutine(MoveCoroutine(startPosition, endPosition, duration));
    }

    private IEnumerator MoveCoroutine(Vector3 startPosition, Vector3 endPosition, float duration){
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

    void Update()
    {
    }

    public ObjectColor getColor()
    {
        return cubeColor;
    }

    public void SetSprite(Sprite sprite)
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = sprite;
        }

    public void SetColor(string color) // I know i should use builder for this BUT BOY am i tired
    {
        // Direct mapping for the color codes used in the grid
        if (color == "r")
            cubeColor = ObjectColor.Red;
        else if (color == "g")
            cubeColor = ObjectColor.Green;
        else if (color == "b")
            cubeColor = ObjectColor.Blue;
        else if (color == "y")
            cubeColor = ObjectColor.Yellow;
        else if (color == "rand")
        {
            // Choose a random color
            cubeColor = (ObjectColor)Random.Range(0, 4);
        }
        else
        {
            // Try to parse the full color name
            if (System.Enum.TryParse(color, true, out ObjectColor parsedColor))
            {
                cubeColor = parsedColor;
            }
            else
            {
                Debug.LogWarning($"Invalid color: {color}");
                // Default to a color to avoid errors
                cubeColor = ObjectColor.Red;
            }
        }
    }
}