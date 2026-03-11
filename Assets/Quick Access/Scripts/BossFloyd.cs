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
    public Health health;

    [Header("Boss Health")]
    public int bossMaxHealth = 20;
    public bool bossGunOneHitKill = false;

    [Header("Combat")]
    public float attackRange = 2.2f;
    public float attackCooldown = 1.1f;
    public int damage = 2;

    [Header("Animation")]
    public string speedParam = "Speed";
    public string attackTrigger = "Punch";
    public float faceTurnSpeed = 10f;

    float nextAttackTime;
    bool dead;

    void Awake()
    {
        if (health == null)
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

        // FORCE Floyd to be a boss and not a normal enemy
        if (health != null)
        {
            health.maxHealth = bossMaxHealth;
            health.currentHealth = bossMaxHealth;
            health.gunOneHitKill = bossGunOneHitKill;
        }

        BossIntroUI intro = FindFirstObjectByType<BossIntroUI>();
        if (intro != null)
            intro.ShowBossName("Floyd 'Promiscuous' Thompson");

        if (health != null)
        {
            health.OnDeath -= OnDeath;
            health.OnDeath += OnDeath;
        }
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= OnDeath;
    }

    void Update()
    {
        if (dead) return;
        if (health != null && health.IsDead) return;
        if (player == null) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (animator != null)
                animator.SetFloat(speedParam, agent.velocity.magnitude);
        }
        else
        {
            agent.isStopped = true;

            if (animator != null)
                animator.SetFloat(speedParam, 0f);

            FacePlayer();
            TryAttack();
        }
    }

    void FacePlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * faceTurnSpeed);
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.ResetTrigger(attackTrigger);
            animator.SetTrigger(attackTrigger);
        }

        Invoke(nameof(DealDamage), 0.35f);
    }

    void DealDamage()
    {
        if (dead) return;
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
            agent.ResetPath();
            agent.enabled = false;
        }
    }
}