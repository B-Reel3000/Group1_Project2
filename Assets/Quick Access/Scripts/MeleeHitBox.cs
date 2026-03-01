using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [Header("Owner")]
    public PlayerMeleeEvents owner;          // drag PlayerMeleeEvents here

    [Header("Hit Settings")]
    public int damage = 1;
    public float hitCooldownPerTarget = 0.0f; // usually 0, because we use per-swing gating

    // Per swing: prevents multi-hits on same enemy during one punch
    HashSet<Health> hitThisSwing = new HashSet<Health>();

    // Optional: if you want extra anti-spam, store time per target
    Dictionary<Health, float> lastHitTime = new Dictionary<Health, float>();

    void Reset()
    {
        // Auto-disable trigger damage at start
        if (owner == null) owner = GetComponentInParent<PlayerMeleeEvents>();
    }

    public void BeginSwing()
    {
        hitThisSwing.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    void OnTriggerStay(Collider other)
    {
        // Helps if fists start inside collider or move fast
        TryHit(other);
    }

    void TryHit(Collider other)
    {
        if (owner == null) return;
        if (!owner.CanDealDamage) return;     // only during active window
        if (other == null) return;

        // Don’t hit yourself
        if (other.GetComponentInParent<PlayerController>() != null) return;

        Health h = other.GetComponentInParent<Health>();
        if (h == null) return;

        // Don’t hit same target multiple times in one swing
        if (hitThisSwing.Contains(h)) return;

        // Optional extra cooldown per target
        if (hitCooldownPerTarget > 0f)
        {
            if (lastHitTime.TryGetValue(h, out float t) && Time.time < t + hitCooldownPerTarget)
                return;

            lastHitTime[h] = Time.time;
        }

        hitThisSwing.Add(h);

        // Deal damage
        h.TakeDamage(damage, Health.DamageType.Melee);

        // Tell owner we landed (optional, for SFX/camera shake/etc.)
        owner.OnLandedHit(h);
    }
}