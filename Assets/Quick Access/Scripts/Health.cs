using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public enum DamageType { Melee, Bullet }

    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Animation")]
    public Animator animator;
    public string hitTrigger = "Hit";
    public string deadBool = "Dead";

    [Header("Role")]
    public bool isPlayer = false; // only true on player

    [Header("Enemy Death Timing")]
    public float destroyDelay = 2.0f; // time for KO to play before disappearing

    [Header("Enemy Death Ground Snap")]
    public LayerMask groundLayers = ~0;
    public float snapRayStartHeight = 1.5f;
    public float snapRayLength = 6f;
    public float groundOffset = 0.02f;

    // WaveManager uses this
    public event Action<Health, DamageType> OnDeath;

    bool dead;

    void Awake()
    {
        currentHealth = maxHealth;
        dead = false;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.SetBool(deadBool, false);
    }

    public void TakeDamage(int amount, DamageType type)
    {
        if (dead) return;
        if (amount <= 0) return;

        currentHealth -= amount;

        if (currentHealth > 0)
        {
            if (animator != null)
                animator.SetTrigger(hitTrigger);
        }
        else
        {
            currentHealth = 0;
            Die(type);
        }
    }

    void Die(DamageType type)
    {
        if (dead) return;
        dead = true;

        if (animator != null)
            animator.SetBool(deadBool, true);

        // Notify WaveManager FIRST
        OnDeath?.Invoke(this, type);

        if (isPlayer)
        {
            // optional: disable controls
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;

            PlayerMelee pm = GetComponent<PlayerMelee>();
            if (pm != null) pm.enabled = false;

            PlayerGun pg = GetComponent<PlayerGun>();
            if (pg != null) pg.enabled = false;

            if (GameManager.Instance != null)
                GameManager.Instance.Lose();

            return;
        }

        // ENEMY cleanup so KO lies correctly:
        // 1) stop nav + physics interference
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 2) snap root to ground so they don't "hover"
        SnapToGround();

        // 3) Destroy after KO has time to show
        Destroy(gameObject, destroyDelay);
    }

    void SnapToGround()
    {
        Vector3 origin = transform.position + Vector3.up * snapRayStartHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, snapRayLength, groundLayers, QueryTriggerInteraction.Ignore))
        {
            Vector3 p = transform.position;
            p.y = hit.point.y + groundOffset;
            transform.position = p;
        }
    }
}