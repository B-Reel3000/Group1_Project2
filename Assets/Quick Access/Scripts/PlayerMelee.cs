using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public Transform attackPoint;
    public Animator animator;
    public string punchTrigger = "Punch";

    [Header("Melee")]
    public float range = 2.2f;
    public float radius = 0.6f;                 // forgiving
    public int damage = 1;
    public float attackCooldown = 0.45f;
    public LayerMask hittableLayers = ~0;

    [Header("Debug")]
    public bool debugLogs = true;

    float nextAttackTime;

    void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (controller == null || attackPoint == null) return;
        if (controller.IsAiming) return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (animator != null) animator.SetTrigger(punchTrigger);

            DoPunch();
        }
    }

    void DoPunch()
    {
        Vector3 origin = attackPoint.position;
        Vector3 dir = transform.forward;

        Debug.DrawRay(origin, dir * range, Color.yellow, 0.25f);

        if (!Physics.SphereCast(origin, radius, dir, out RaycastHit hit, range, hittableLayers, QueryTriggerInteraction.Ignore))
        {
            if (debugLogs) Debug.Log("[PUNCH] No hit (SphereCast missed).");
            return;
        }

        if (debugLogs)
            Debug.Log($"[PUNCH] Hit: {hit.collider.name} (tag={hit.collider.tag}, layer={LayerMask.LayerToName(hit.collider.gameObject.layer)})");

        // skip self
        if (hit.collider.GetComponentInParent<PlayerController>() != null || hit.collider.transform.IsChildOf(transform))
        {
            if (debugLogs) Debug.Log("[PUNCH] Ignored self collider.");
            return;
        }

        Health h = hit.collider.GetComponentInParent<Health>();
        if (h == null)
        {
            if (debugLogs) Debug.Log("[PUNCH] HIT SOMETHING but found NO Health in parent chain. Put Health on the enemy ROOT (or parent of this collider).");
            return;
        }

        if (debugLogs) Debug.Log($"[PUNCH] Damaging Health on: {h.gameObject.name} for {damage}");
        h.TakeDamage(damage, Health.DamageType.Melee);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, radius);
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + transform.forward * range);
    }
}