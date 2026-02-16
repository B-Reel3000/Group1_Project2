using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHealth = 5;
    int current;

    // tells us if it was melee or gun
    public enum DamageType { Melee, Gun }
    DamageType lastDamageType;

    public Action<DamageType> OnDeath;

    void Awake()
    {
        current = maxHealth;
    }

    public void TakeDamage(int amount, DamageType type)
    {
        lastDamageType = type;

        current -= amount;
        if (current <= 0)
            Die();
    }

    void Die()
    {
        OnDeath?.Invoke(lastDamageType);
        gameObject.SetActive(false);
    }
}
