using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMelee : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public Transform attackPoint; // empty object at chest/hand level

    [Header("Melee Settings")]
    public float attackRadius = 1.4f;      // size of hit area
    public int damage = 1;
    public float attackCooldown = 0.45f;
    public LayerMask enemyLayers;

    [Header("Animation")]
    public Animator animator;
    public string punchTrigger = "Punch";

    float nextAttackTime;

    void Update()
    {
        if (controller == null || attackPoint == null)
            return;

        // No punching while aiming gun
        if (controller.IsAiming)
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(DoPunch());
        }
    }

    IEnumerator DoPunch()
    {
        // play animation
        if (animator != null)
            animator.SetTrigger(punchTrigger);

        // IMPORTANT:
        // wait for fist to actually reach forward in the animation
        yield return new WaitForSeconds(0.12f);

        // sphere hit detection
        Collider[] hits = Physics.OverlapSphere(
            attackPoint.position,
            attackRadius,
            enemyLayers,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Health h = hits[i].GetComponentInParent<Health>();
            if (h != null)
            {
                h.TakeDamage(damage, Health.DamageType.Melee);
            }
        }
    }

    // visualize in editor
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}