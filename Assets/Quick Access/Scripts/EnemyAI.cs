using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public Animator animator;
    public Health health;

    [Header("Combat")]
    public float attackRange = 2.2f;
    public float attackCooldown = 1.2f;
    public int damage = 1;

    float nextAttackTime;
    NavMeshAgent agent;
    bool dead;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (dead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // movement
        if (dist > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f);

            // face player
            Vector3 dir = (player.position - transform.position);
            dir.y = 0f;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                   Quaternion.LookRotation(dir),
                                                   Time.deltaTime * 8f);

            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        animator.SetTrigger("Punch");
        Invoke(nameof(DealDamage), 0.35f); // timing of punch impact
    }

    void DealDamage()
    {
        if (dead) return;
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.3f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage);
        }
    }

    public void Die()
    {
        if (dead) return;
        dead = true;

        agent.isStopped = true;
        agent.enabled = false;

        animator.SetBool("Dead", true);

        Destroy(gameObject, 3f); // wait for animation
    }
}