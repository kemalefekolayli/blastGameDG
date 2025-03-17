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
        if (gridManager == null)
        {
            Debug.LogError("GridManager is NULL in CubeObject!");
            return;
        }

        if (isAnimating) return;

        var matchingGroup = gridManager.FindMatchingGroup(gridPosition);

        if (matchingGroup.Count >= 2)
        {
            foreach (var pos in matchingGroup)
            {
                gridManager.RemoveCube(pos);
            }
        }
        else
        {
            Debug.Log("Not enough matching cubes to blast");
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

    public void SetColor(string colorCode)
    {
        switch(colorCode.ToLower())
        {
            case "r": cubeColor = ObjectColor.Red; break;
            case "g": cubeColor = ObjectColor.Green; break;
            case "b": cubeColor = ObjectColor.Blue; break;
            case "y": cubeColor = ObjectColor.Yellow; break;
            default: Debug.LogWarning($"Unknown color code: {colorCode}"); break;
        }
    }
}