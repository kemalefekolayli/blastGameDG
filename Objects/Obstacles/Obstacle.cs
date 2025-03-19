using UnityEngine;

public abstract class Obstacle : MonoBehaviour, IGridObject, IDamageable
{
    protected GridManager gridManager;
    protected Vector2Int gridPosition;
    protected int health;
    protected bool isDestroyed = false;
    protected SpriteRenderer spriteRenderer;

    // Properties
    public bool IsDestroyed => isDestroyed;
    public bool CanFall { get; protected set; }  // Set in derived classes

    // IGridObject implementation
    public virtual void Initialize(GridManager gridManager, Vector2Int position)
    {
        this.gridManager = gridManager;
        this.gridPosition = position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void DoWork() { }

    // IDamageable implementation
    public abstract bool CanTakeDamage(DamageType damageType);

    public virtual void TakeDamage(DamageType damageType, int amount)
    {
        if (!CanTakeDamage(damageType)) return;

        health -= amount;
        if (health <= 0)
        {
            isDestroyed = true;
            gridManager.RemoveObstacle(gridPosition);
        }
        else
        {
            // Show visual damage
            UpdateVisuals();
        }
    }

    // Helper methods
    protected abstract void UpdateVisuals();
}