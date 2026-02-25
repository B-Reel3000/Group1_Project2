using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Animation")]
    public Animator animator;          // drag model Animator here
    public string hitTrigger = "Hit";  // trigger in Animator
    public string deadBool = "Dead";   // bool in Animator

    bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

        // Optional: ensure we start alive if you reuse prefab
        if (animator != null) animator.SetBool(deadBool, false);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int newHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        // If this hit kills, go straight to KO (do NOT trigger Hit)
        if (newHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        // Non-lethal hit
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

        if (GameManager.Instance != null)
            GameManager.Instance.Lose();

        // Optional: disable player controls
        // GetComponent<PlayerController>().enabled = false;
        // GetComponent<PlayerMelee>().enabled = false;
    }
}