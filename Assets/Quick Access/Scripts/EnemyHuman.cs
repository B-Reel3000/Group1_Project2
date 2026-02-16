using UnityEngine;

[RequireComponent(typeof(Health))]
public class HumanEnemy : MonoBehaviour
{
    public GameObject ammoPrefab;
    public float dropChance = 0.5f;

    Health health;

    void Start()
    {
        health = GetComponent<Health>();
        health.OnDeath += OnKilled;
    }

    void OnKilled(Health.DamageType type)
    {
        // only drop if killed by melee
        if (type != Health.DamageType.Melee) return;

        if (Random.value <= dropChance)
        {
            Instantiate(ammoPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
    }
}
