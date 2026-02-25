// MeleeHitbox.cs  (optional debug added)
using UnityEngine;
using System.Collections.Generic;

public class MeleeHitbox : MonoBehaviour
{
    [Header("Hitbox")]
    public int damage = 1;
    public string enemyTag = "Enemy";

    [Header("Debug")]
    public bool debugLogs = false;

    [HideInInspector] public bool active;

    HashSet<Health> alreadyHit = new HashSet<Health>();

    public void ResetHits()
    {
        alreadyHit.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!active) return;
        if (!other.CompareTag(enemyTag)) return;

        Health h = other.GetComponentInParent<Health>();
        if (h == null) return;

        if (alreadyHit.Contains(h)) return;
        alreadyHit.Add(h);

        if (debugLogs) Debug.Log("HIT ENEMY: " + other.name);

        h.TakeDamage(damage, Health.DamageType.Melee);
    }
}