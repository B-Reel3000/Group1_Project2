using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerMelee : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public Transform attackPoint; // empty child in front of chest

    [Header("Melee")]
    public float range = 2.2f;
    public int damage = 1;              // 1 per punch (so 5 punches if Health maxHealth=5)
    public float attackCooldown = 0.45f;

    [Header("Debug")]
    public bool debugLogs = true;
    public bool drawRay = true;

    float nextAttackTime;

    void Update()
    {
        if (controller == null || attackPoint == null) return;

        // Only punch in melee mode
        if (controller.IsAiming) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            Punch();
        }
    }

    void Punch()
    {
        Vector3 origin = attackPoint.position;
        Vector3 dir = transform.forward; // use player forward (more reliable than attackPoint.forward)

        if (drawRay)
            Debug.DrawRay(origin, dir * range, Color.yellow, 0.25f);

        // RaycastAll so we can skip our own colliders
        RaycastHit[] hits = Physics.RaycastAll(origin, dir, range);
        if (hits == null || hits.Length == 0)
        {
            if (debugLogs) Debug.Log("Punch: no hit");
            return;
        }

        // Sort nearest first
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            // Skip anything on the player (prevents self-hit)
            if (hit.collider.GetComponentInParent<PlayerController>() != null ||
                hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (debugLogs) Debug.Log("Punch hit: " + hit.collider.name);

            Health h = hit.collider.GetComponentInParent<Health>();
            if (h != null)
            {
                if (debugLogs) Debug.Log("Punch -> damaging Health on: " + h.gameObject.name);
                h.TakeDamage(damage, Health.DamageType.Melee);
            }
            else
            {
                if (debugLogs) Debug.Log("Punch hit object has no Health.");
            }

            return; // only hit one thing per punch
        }

        if (debugLogs) Debug.Log("Punch: only hit self colliders (skipped).");
    }
}
