using System;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour
{
    public enum DamageType { Melee, Gun }

    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Gun Rules")]
    public bool gunOneHitKill = true;

    [Header("Animation (Optional)")]
    public Animator animator;
    public string hitTrigger = "Hit";
    public string deadBool = "Dead";
    public float destroyDelay = 1.5f;

    [Header("Death Fixes")]
    public bool disableAIAndNavOnDeath = true;
    public bool disableCollidersOnDeath = true;

    [Tooltip("Snap enemy so collider feet sit on ground (fixes floating death anim).")]
    public bool snapFeetToGroundOnDeath = true;
    public LayerMask groundLayers = ~0;
    public float snapRayStartHeight = 2.0f;
    public float snapRayLength = 8.0f;
    public float groundOffset = 0.02f;

    public event Action<Health, DamageType> OnDeath;

    bool dead;

    void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int amount, DamageType type)
    {
        if (dead) return;
        if (amount <= 0) return;

        if (type == DamageType.Gun && gunOneHitKill)
        {
            currentHealth = 0;
            Die(type);
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
        {
            animator.ResetTrigger(hitTrigger);
            animator.SetTrigger(hitTrigger);
        }

        if (currentHealth <= 0)
            Die(type);
    }

    void Die(DamageType type)
    {
        if (dead) return;
        dead = true;

        if (animator != null && !string.IsNullOrEmpty(deadBool))
            animator.SetBool(deadBool, true);

        // ✅ Hard stop AI + Nav so they cannot chase while "dying"
        if (disableAIAndNavOnDeath)
        {
            var ai = GetComponent<EnemyAI_Navmesh>();
            if (ai == null) ai = GetComponentInParent<EnemyAI_Navmesh>();
            if (ai != null) ai.OnDeath();

            var agent = GetComponent<NavMeshAgent>();
            if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.enabled = false;
            }
        }

        // ✅ Fix floating by snapping FEET (collider bounds min)
        if (snapFeetToGroundOnDeath)
            SnapFeetToGround();

        if (disableCollidersOnDeath)
        {
            var cols = GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
                cols[i].enabled = false;
        }

        OnDeath?.Invoke(this, type);

        Destroy(gameObject, destroyDelay);
    }

    void SnapFeetToGround()
    {
        // Find lowest collider point (feet)
        var cols = GetComponentsInChildren<Collider>();
        if (cols == null || cols.Length == 0) return;

        float lowestY = float.PositiveInfinity;
        for (int i = 0; i < cols.Length; i++)
        {
            if (!cols[i].enabled) continue;
            lowestY = Mathf.Min(lowestY, cols[i].bounds.min.y);
        }

        Vector3 origin = transform.position + Vector3.up * snapRayStartHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, snapRayLength, groundLayers, QueryTriggerInteraction.Ignore))
        {
            // Move root so collider bottoms sit on ground hit point
            float delta = hit.point.y - lowestY;
            transform.position += new Vector3(0f, delta + groundOffset, 0f);
        }
    }

    public bool IsDead => dead;
}