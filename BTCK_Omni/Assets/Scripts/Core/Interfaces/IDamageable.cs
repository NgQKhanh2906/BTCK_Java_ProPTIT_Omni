using System;
using UnityEngine;

public interface IDamageable
{
    float CurrentHP { get; }
    float MaxHP     { get; }
    bool  IsDead();
    void  TakeDamage(float dmg, Vector2 hitDir);
    void  Die();
    event Action<float, float> OnHPChanged;
    event Action OnDeath;
}