using UnityEngine;
// Vase Obstacle Implementation
public class VaseObstacle : Obstacle, IAnimatableObject
{
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite damagedSprite;

    private bool isAnimating = false;

    public bool IsAnimating => isAnimating;

    private void Awake()
    {
        health = 2;
        CanFall = true;
    }

    public override void Initialize(GridManager gridManager, Vector2Int position)
    {
        base.Initialize(gridManager, position);

        if (spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    // Vase takes damage from both adjacent blasts and rockets
    public override bool CanTakeDamage(DamageType damageType)
    {
        return true;
    }

    // Override TakeDamage to implement the one-damage-per-blast rule
    public override void TakeDamage(DamageType damageType, int amount)
    {
        if (!CanTakeDamage(damageType)) return;

        // Vase takes at most 1 damage from a single blast
        if (damageType == DamageType.Adjacent)
        {
            amount = 1;
        }

        base.TakeDamage(damageType, amount);
    }

    protected override void UpdateVisuals()
    {
        // Update sprite to show damage
        if (health == 1 && spriteRenderer != null && damagedSprite != null)
        {
            spriteRenderer.sprite = damagedSprite;
        }
    }

    // Implementation for IAnimatableObject for falling
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