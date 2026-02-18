using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHealth = 5;
    int current;

    public enum DamageType { Melee, Gun }

    public event Action<Health, DamageType> OnDeath;

    void Awake()
    {
        current = maxHealth;
    }

    public void TakeDamage(int amount, DamageType type)
    {
        current -= amount;

        if (current <= 0)
            Die(type);
    }

    void Die(DamageType type)
    {
        // IMPORTANT: send BOTH the enemy and the damage type
        OnDeath?.Invoke(this, type);

        gameObject.SetActive(false);
    }
}
