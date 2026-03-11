using UnityEngine;

public class DeathVisualClamp : MonoBehaviour
{
    [Header("Optional")]
    public Animator animator;
    public string deadBool = "Dead";

    [Header("Clamp")]
    [Tooltip("If true, captures the starting local Y on Awake.")]
    public bool captureStartYOnAwake = true;

    [Tooltip("Lowest local Y the model is allowed to go while dead.")]
    public float minLocalY;

    [Tooltip("Extra allowance below start Y. Usually leave at 0.")]
    public float extraDropAllowance = 0f;

    bool isDead;
    bool hasCaptured;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (captureStartYOnAwake)
        {
            minLocalY = transform.localPosition.y;
            hasCaptured = true;
        }
    }

    void LateUpdate()
    {
        if (!isDead)
        {
            if (animator != null)
            {
                // Detect death from animator bool
                if (animator.GetBool(deadBool))
                    isDead = true;
            }
        }

        if (!isDead) return;

        Vector3 p = transform.localPosition;
        float clampY = hasCaptured ? (minLocalY - extraDropAllowance) : minLocalY;

        if (p.y < clampY)
        {
            p.y = clampY;
            transform.localPosition = p;
        }
    }
}