// PlayerHealth.cs
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Animation")]
    public Animator animator;          // drag model Animator here
    public string hitTrigger = "Hit";  // trigger in Animator
    public string deadBool = "Dead";   // bool in Animator

    [Header("Feel")]
    [Tooltip("Minimum time between taking damage (prevents hit spam).")]
    public float damageCooldown = 0.6f;

    bool isDead;
    float nextDamageTime;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        nextDamageTime = 0f;

        if (animator != null)
            animator.SetBool(deadBool, false);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        if (Time.time < nextDamageTime) return;
        nextDamageTime = Time.time + damageCooldown;

        int newHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        // If this hit kills, go straight to KO (do NOT trigger Hit)
        if (newHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        currentHealth = newHealth;

        if (animator != null)
            animator.SetTrigger(hitTrigger);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player died!");

        if (animator != null)
            animator.SetBool(deadBool, true);

        // Stop physics so the KO pose doesn't "hover" or slide
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.Lose();

        // Optional: disable player controls
        // GetComponent<PlayerController>().enabled = false;
        // GetComponent<PlayerMelee>().enabled = false;
    }
}