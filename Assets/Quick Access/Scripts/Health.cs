using System;
using UnityEngine;

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
    [Tooltip("If true, any Gun damage instantly kills this Health.")]
    public bool gunOneHitKill = true;

    [Header("Animation (Optional)")]
    public Animator animator;
    public string hitTrigger = "Hit";
    public string deadBool = "Dead";
    public float destroyDelay = 1.5f;

    [Header("Death Offset Fix")]
    [Tooltip("Assign the visual model root (child) that should shift down on death to avoid floating.")]
    public Transform visualRoot;
    [Tooltip("Negative values usually push the model down onto the floor.")]
    public float deathYOffset = -0.35f;

    // Fired whenever damage is taken (even if it doesn't kill)
    public event Action<Health, DamageType, int> OnDamaged; // (who, type, amount)

    // Fired when health reaches 0
    public event Action<Health, DamageType> OnDeath;

    bool dead;

    void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Convenience: if visualRoot not assigned, try to use animator's transform (often the model)
        if (visualRoot == null && animator != null)
            visualRoot = animator.transform;
    }

    public void TakeDamage(int amount, DamageType type)
    {
        if (dead) return;
        if (amount <= 0) return;

        // Gun instant kill option
        if (type == DamageType.Gun && gunOneHitKill)
        {
            OnDamaged?.Invoke(this, type, amount);
            currentHealth = 0;
            Die(type);
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        // Hit reaction trigger (more reliable with ResetTrigger)
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

        // Push the visual model down so death animations don't "float"
        if (visualRoot != null)
        {
            Vector3 p = visualRoot.localPosition;
            p.y += deathYOffset;
            visualRoot.localPosition = p;
        }

        if (animator != null && !string.IsNullOrEmpty(deadBool))
            animator.SetBool(deadBool, true);

        OnDeath?.Invoke(this, type);

        Destroy(gameObject, destroyDelay);
    }

    public bool IsDead => dead;
}