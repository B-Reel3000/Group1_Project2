using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Animation")]
    public Animator animator;
    public string hitTrigger = "Hit";
    public string deadBool = "Dead";

    [Header("Feel")]
    public float damageCooldown = 0.6f;

    [Header("Death")]
    public float deathFreezeY = 0.02f;

    bool isDead;
    float nextDamageTime;

    Rigidbody rb;
    CapsuleCollider capsule;
    PlayerController controller;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        nextDamageTime = 0f;

        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        controller = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetBool(deadBool, false);
            animator.applyRootMotion = false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        if (Time.time < nextDamageTime) return;
        nextDamageTime = Time.time + damageCooldown;

        int newHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        if (newHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        currentHealth = newHealth;

        if (animator != null && !string.IsNullOrEmpty(hitTrigger))
        {
            animator.ResetTrigger(hitTrigger);
            animator.SetTrigger(hitTrigger);
        }
    }

    public void KillInstant()
    {
        if (isDead) return;

        currentHealth = 0;
        nextDamageTime = 0f;
        Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
            animator.SetBool(deadBool, true);

        if (controller != null)
            controller.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Keep the body from dropping or drifting
        Vector3 p = transform.position;
        p.y = deathFreezeY;
        transform.position = p;

        if (capsule != null)
            capsule.enabled = true;

        if (GameManager.Instance != null)
            GameManager.Instance.Lose();
    }
}