using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public Animator animator;                 // drag the model Animator here
    public string punchTriggerName = "Punch"; // must match Animator parameter

    [Header("Melee")]
    public float attackCooldown = 0.45f;

    float nextAttackTime;

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (controller == null) return;

        // Only punch in melee mode
        if (controller.IsAiming) return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (animator != null)
                animator.SetTrigger(punchTriggerName);
        }
    }
}