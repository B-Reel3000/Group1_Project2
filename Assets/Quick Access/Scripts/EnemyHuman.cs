using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHuman : MonoBehaviour
{
    public GameObject ammoPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.5f;

    Health health;

    void Start()
    {
        health = GetComponent<Health>();
        health.OnDeath += OnKilled;
    }

    void OnDestroy()
    {
        // Prevent event leaks if object gets destroyed
        if (health != null)
            health.OnDeath -= OnKilled;
    }

    // MUST match: Action<Health, Health.DamageType>
    void OnKilled(Health whoDied, Health.DamageType type)
    {
        // only drop if killed by melee
        if (type != Health.DamageType.Melee) return;

        if (ammoPrefab != null && Random.value <= dropChance)
        {
            Instantiate(ammoPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
    }
}
