using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMeleeEvents : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public Animator animator;

    [Header("Animator Params")]
    public string punchTrigger = "Punch"; // trigger
    public string speedParam = "Speed";   // optional (already in your controller)

    [Header("Timing")]
    public float attackCooldown = 0.45f;

    [Header("State")]
    public bool CanDealDamage { get; private set; }

    float nextAttackTime;

    // Assign both fists in Inspector (each has MeleeHitbox)
    [Header("Hitboxes")]
    public MeleeHitbox leftFist;
    public MeleeHitbox rightFist;

    void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (controller == null) return;

        // Only punch in melee mode (not aiming)
        if (controller.IsAiming) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            // Start punch
            if (animator != null)
                animator.SetTrigger(punchTrigger);

            // Prepare hitboxes for a new swing
            if (leftFist != null) leftFist.BeginSwing();
            if (rightFist != null) rightFist.BeginSwing();
        }
    }

    // --- Animation Events call these ---

    // Call this at the moment the fist should start hurting
    public void AE_DamageOn()
    {
        CanDealDamage = true;
    }

    // Call this right after the punch impact frames end
    public void AE_DamageOff()
    {
        CanDealDamage = false;
    }

    // Optional: if you want to guarantee off at end of clip
    public void AE_EndSwing()
    {
        CanDealDamage = false;
    }

    // Optional callback for feedback
    public void OnLandedHit(Health target)
    {
        // Add SFX, camera shake, hit-stop, UI, etc later
        // Debug.Log("Punched: " + target.name);
    }
}