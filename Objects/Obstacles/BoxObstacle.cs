

// Box Obstacle Implementation
public class BoxObstacle : Obstacle
{
    [SerializeField] private Sprite defaultSprite;

    private void Awake()
    {
        health = 1;
        CanFall = false;
    }

    public override void Initialize(GridManager gridManager, Vector2Int position)
    {
        base.Initialize(gridManager, position);

        if (spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    // Box takes damage from both adjacent blasts and rockets
    public override bool CanTakeDamage(DamageType damageType)
    {
        return true; // Can take damage from any source
    }

    protected override void UpdateVisuals()
    {
        // Box has only 1 health, so no need for damaged visuals
    }
}

