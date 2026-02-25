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
    public float snapRayStartHeight = 1.5f;   // start ray above player
    public float snapRayLength = 5f;          // how far down to look
    public float groundOffset = 0.02f;        // tiny lift so we don't clip

    bool isDead;
    float nextDamageTime;

    Rigidbody rb;
    CapsuleCollider capsule;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        nextDamageTime = 0f;

        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

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

        // lethal hit: no Hit trigger
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

        // play KO
        if (animator != null)
            animator.SetBool(deadBool, true);

        // Disable capsule so it doesn't keep the body "standing"
        if (capsule != null)
            capsule.enabled = false;

        // Stop physics motion
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Snap root down to ground so KO rests on floor
        SnapToGround();

        if (GameManager.Instance != null)
            GameManager.Instance.Lose();

        // Optional: disable controls
        // GetComponent<PlayerController>().enabled = false;
        // GetComponent<PlayerMelee>().enabled = false;
    }

    void SnapToGround()
    {
        Vector3 origin = transform.position + Vector3.up * snapRayStartHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, snapRayLength, groundLayers, QueryTriggerInteraction.Ignore))
        {
            // Place root on the ground hit point (plus a tiny offset)
            Vector3 p = transform.position;
            p.y = hit.point.y + groundOffset;
            transform.position = p;
        }
    }
}