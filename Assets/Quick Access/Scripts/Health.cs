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

    [Header("Death")]
    public bool isPlayer = false;   // CHECK this on the player only
    public float destroyDelay = 3f; // enemy disappears after death anim

    // ✅ WaveManager (and anything else) can subscribe to this
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

        // Hit reaction only if still alive
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

        // play death animation
        if (animator != null)
            animator.SetBool(deadBool, true);

        // ✅ notify listeners (WaveManager)
        OnDeath?.Invoke(this, type);

        // PLAYER
        if (isPlayer)
        {
            // Disable controls (optional)
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

        // ENEMY: let AI handle stopping, etc.
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
            ai.Die();

        Destroy(gameObject, destroyDelay);
    }
}