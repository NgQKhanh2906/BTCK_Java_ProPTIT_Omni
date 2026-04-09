using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent
{
    float damage;
    public DamageComponent(float damage)
    {
        this.damage = damage;
    }

    public void Attack(ITakeDamageable target)
    {
        target.TakeDamage(this.damage);
    }
}