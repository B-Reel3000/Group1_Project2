using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;          // model animator
    public Rigidbody rootRigidbody;    // the main capsule rigidbody (PlayerPhase2)
    public Collider rootCollider;      // the main capsule collider (PlayerPhase2)

    [Header("Ragdoll Parts (auto-filled at runtime)")]
    Rigidbody[] ragdollBodies;
    Collider[] ragdollColliders;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Collect ragdoll RB/Colliders from children (bones)
        ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);

        // Make sure ragdoll is OFF at start
        SetRagdoll(false);
    }

    public void SetRagdoll(bool enabled)
    {
        // Toggle Animator
        if (animator != null)
            animator.enabled = !enabled;

        // Toggle main body (capsule)
        if (rootRigidbody != null)
        {
            rootRigidbody.linearVelocity = Vector3.zero;
            rootRigidbody.angularVelocity = Vector3.zero;
            rootRigidbody.isKinematic = enabled;  // when ragdoll ON, stop driving capsule
        }

        if (rootCollider != null)
            rootCollider.enabled = !enabled;

        // Toggle all ragdoll bones (skip the rootRigidbody if it�s included)
        for (int i = 0; i < ragdollBodies.Length; i++)
        {
            Rigidbody rb = ragdollBodies[i];

            if (rb == null) continue;
            if (rootRigidbody != null && rb == rootRigidbody) continue;

            rb.isKinematic = !enabled;
            rb.useGravity = enabled;

            if (enabled)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        for (int i = 0; i < ragdollColliders.Length; i++)
        {
            Collider c = ragdollColliders[i];
            if (c == null) continue;

            // Keep the capsule collider controlled separately
            if (rootCollider != null && c == rootCollider) continue;

            c.enabled = enabled;
        }
    }
}