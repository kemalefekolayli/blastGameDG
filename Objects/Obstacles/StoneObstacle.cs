using UnityEngine;
// Stone Obstacle Implementation

public class StoneObstacle : Obstacle
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

    // Stone only takes damage from rockets
    public override bool CanTakeDamage(DamageType damageType)
    {
        return damageType == DamageType.Rocket;
    }

    protected override void UpdateVisuals()
    {
        // Stone has only 1 health, so no need for damaged visuals
    }
}