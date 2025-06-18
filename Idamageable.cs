// IDamageable.cs
public interface IDamageable
{
    // O contrato agora exige dois métodos
    void TakeDamage(float damageAmount);
    bool IsDead();
}