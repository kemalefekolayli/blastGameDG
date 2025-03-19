public enum DamageType { Adjacent, Rocket }

public interface IDamageable
{
    bool CanTakeDamage(DamageType damageType);
    void TakeDamage(DamageType damageType, int amount);
    bool IsDestroyed { get; }
}
