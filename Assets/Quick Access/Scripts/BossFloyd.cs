using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class BossFloyd : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public NavMeshAgent agent;

    [Header("Combat")]
    public float attackRange = 2.2f;
    public float attackCooldown = 1.1f;
    public int damage = 2;

    float nextAttackTime;

    Health health;
    bool dead;

    void Awake()
    {
        health = GetComponent<Health>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc) player = pc.transform;
        }

        // Floyd intro
        BossIntroUI intro = FindFirstObjectByType<BossIntroUI>();
        if (intro != null)
            intro.ShowBossName("Floyd 'The Bruiser' Thompson");

        health.OnDeath += OnDeath;
    }

    void Update()
    {
        if (dead) return;
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (animator != null)
                animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            agent.isStopped = true;

            if (animator != null)
                animator.SetFloat("Speed", 0);

            FacePlayer();

            TryAttack();
        }
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 10f
        );
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        if (animator != null)
            animator.SetTrigger("Punch");

        Invoke(nameof(DealDamage), 0.35f);
    }

    void DealDamage()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange + 0.4f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();

            if (ph != null)
                ph.TakeDamage(damage);
        }
    }

    void OnDeath(Health h, Health.DamageType type)
    {
        dead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
    }
}