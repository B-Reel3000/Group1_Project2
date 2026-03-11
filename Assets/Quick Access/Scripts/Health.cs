using System;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour
{
    public enum DamageType
    {
        Melee,
        Gun
    }

    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Gun Rules")]
    public bool gunOneHitKill = true;

    [Header("Animation (Optional)")]
    public Animator animator;
    public string hitTrigger = "Hit";
    public string deadBool = "Dead";

    [Header("Death Handling")]
    [Tooltip("If true, enemy is destroyed immediately on death. Best deadline-safe option.")]
    public bool destroyImmediatelyOnDeath = true;

    [Tooltip("Used only if destroyImmediatelyOnDeath is false.")]
    public float destroyDelay = 0.15f;

    public event Action<Health, DamageType, int> OnDamaged;
    public event Action<Health, DamageType> OnDeath;

    bool dead;

    void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;
    }

    public void TakeDamage(int amount, DamageType type)
    {
        if (dead) return;
        if (amount <= 0) return;

        if (type == DamageType.Gun && gunOneHitKill)
        {
            OnDamaged?.Invoke(this, type, amount);
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

        OnDamaged?.Invoke(this, type, amount);

        if (currentHealth <= 0)
            Die(type);
    }

    void Die(DamageType type)
    {
        if (dead) return;
        dead = true;

        // Stop AI immediately
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = GetComponentInParent<NavMeshAgent>();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        EnemyAI_Navmesh ai = GetComponent<EnemyAI_Navmesh>();
        if (ai == null) ai = GetComponentInParent<EnemyAI_Navmesh>();

        if (ai != null)
            ai.OnDeath();

        // Disable all trigger hitboxes so dead enemy cannot still damage player
        Collider[] allCols = GetComponentsInChildren<Collider>();
        for (int i = 0; i < allCols.Length; i++)
        {
            if (allCols[i].isTrigger)
                allCols[i].enabled = false;
        }

        // Fire death event first so WaveManager / boss logic still works
        OnDeath?.Invoke(this, type);

        // Optional death anim if you really want a tiny beat before deleting
        if (animator != null && !string.IsNullOrEmpty(deadBool))
            animator.SetBool(deadBool, true);

        if (destroyImmediatelyOnDeath)
            Destroy(gameObject);
        else
            Destroy(gameObject, destroyDelay);
    }

    public bool IsDead => dead;
}