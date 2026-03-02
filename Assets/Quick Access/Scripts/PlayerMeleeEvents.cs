using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMeleeEvents : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public Animator animator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip whooshClip;

    [Header("Animator")]
    public string punchTrigger = "Punch";

    [Header("Timing")]
    public float attackCooldown = 0.5f;
    public float damageOnDelay = 0.10f;
    public float damageActiveTime = 0.18f;

    [Header("Hitboxes")]
    public MeleeHitbox leftFist;
    public MeleeHitbox rightFist;

    public bool CanDealDamage { get; private set; }

    float nextAttackTime;
    Coroutine windowRoutine;

    void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (controller != null && controller.IsAiming) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            // PLAY WHOOSH
            if (audioSource != null && whooshClip != null)
                audioSource.PlayOneShot(whooshClip);

            // animation
            if (animator != null)
                animator.SetTrigger(punchTrigger);

            if (leftFist != null) leftFist.BeginSwing();
            if (rightFist != null) rightFist.BeginSwing();

            if (windowRoutine != null) StopCoroutine(windowRoutine);
            windowRoutine = StartCoroutine(DamageWindow());
        }
    }

    IEnumerator DamageWindow()
    {
        CanDealDamage = false;
        yield return new WaitForSeconds(damageOnDelay);

        CanDealDamage = true;
        yield return new WaitForSeconds(damageActiveTime);

        CanDealDamage = false;
    }

    public void OnLandedHit(Health target)
    {
        // Impact handled in hitbox
    }
}