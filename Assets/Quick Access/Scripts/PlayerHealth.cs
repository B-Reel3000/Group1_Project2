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

    [Header("Death Ground Snap")]
    public LayerMask groundLayers = ~0;
    public float snapRayStartHeight = 1.5f;
    public float snapRayLength = 5f;
    public float groundOffset = 0.08f;

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

        if (animator != null)
            animator.SetTrigger(hitTrigger);
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

        Debug.Log("Player died!");

        if (animator != null)
            animator.SetBool(deadBool, true);

        // DO NOT disable capsule or the body can sink/clip
        if (capsule != null)
            capsule.enabled = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (controller != null)
            controller.enabled = false;

        SnapToGround();

        if (GameManager.Instance != null)
            GameManager.Instance.Lose();
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