using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_Navmesh : MonoBehaviour
{
    public enum State { Chase, Orbit, Attack, Recover }
    public State state;

    [Header("Target")]
    public Transform player;

    [Header("References")]
    public Animator animator;
    public Health health;

    [Header("NavMesh")]
    public float repathRate = 0.15f;

    [Header("Distances")]
    public float orbitEnterDistance = 4.0f;
    public float orbitExitDistance  = 7.0f;
    public float attackDistance     = 1.6f;

    [Header("Orbit")]
    public float orbitRadius = 3.5f;
    public float orbitSpeedDegrees = 60f;

    [Header("Attack")]
    public float attackWindup = 0.18f;        // when damage window starts
    public float damageActiveTime = 0.18f;    // how long fists can deal damage
    public float attackCooldown = 1.25f;
    public int damage = 1;                    // default damage for fists

    [Header("Anti-Dogpile")]
    [Range(1, 4)] public int maxAttackers = 1;

    [Header("Animation")]
    public string speedParam = "Speed";
    public string attackTrigger = "Punch";
    public float faceTurnSpeed = 10f;

    [Header("Fist Hitboxes (assign these)")]
    public EnemyFistHitbox leftFist;
    public EnemyFistHitbox rightFist;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip whooshClip;
    public float whooshVolume = 1f;

    public bool CanDealDamage { get; private set; }

    NavMeshAgent agent;
    float nextAttackTime;
    float nextRepathTime;

    int id;
    float orbitAngle;
    float orbitDrift;

    // If you want to keep your original static anti-dogpile, keep this:
    static readonly System.Collections.Generic.HashSet<int> attackerIds =
        new System.Collections.Generic.HashSet<int>();

    bool dead;
    Coroutine attackRoutine;
    bool hasSlot;

    void Start()
    {
        id = GetInstanceID();
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc) player = pc.transform;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (health == null)
            health = GetComponent<Health>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        orbitAngle = Random.Range(0f, 360f);
        orbitDrift = Random.Range(0.7f, 1.3f);

        CanDealDamage = false;
        state = State.Chase;

        // Make sure fists know owner + damage
        if (leftFist != null)
        {
            leftFist.owner = this;
            leftFist.damage = damage;
        }

        if (rightFist != null)
        {
            rightFist.owner = this;
            rightFist.damage = damage;
        }

        // Stop chasing instantly on death
        if (health != null)
        {
            health.OnDeath -= OnDied;
            health.OnDeath += OnDied;
        }
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= OnDied;
    }

    void Update()
    {
        if (dead) return;
        if (health != null && health.IsDead) return;
        if (player == null) return;
        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Animator speed
        if (animator != null)
            animator.SetFloat(speedParam, agent.velocity.magnitude);

        switch (state)
        {
            case State.Chase:
                SetDestinationThrottled(player.position);

                if (dist <= orbitEnterDistance)
                    state = State.Orbit;
                break;

            case State.Orbit:
                if (dist > orbitExitDistance)
                {
                    ReleaseSlot();
                    state = State.Chase;
                    break;
                }

                // Try to start an attack if off cooldown and can claim slot
                if (Time.time >= nextAttackTime && dist <= orbitEnterDistance && TryClaimSlot())
                {
                    state = State.Attack;
                    break;
                }

                Orbit();
                break;

            case State.Attack:
                if (dist > orbitExitDistance)
                {
                    ReleaseSlot();
                    state = State.Chase;
                    break;
                }

                FacePlayer();

                if (dist > attackDistance)
                {
                    SetDestinationThrottled(player.position);
                }
                else
                {
                    agent.ResetPath();

                    // Start attack once (no stacking)
                    if (attackRoutine == null)
                        attackRoutine = StartCoroutine(DoAttack());

                    state = State.Recover;
                }
                break;

            case State.Recover:
                // waiting for coroutine to finish
                FacePlayer();
                break;
        }
    }

    IEnumerator DoAttack()
    {
        // freeze movement during strike
        agent.isStopped = true;

        // whoosh
        if (audioSource != null && whooshClip != null)
            audioSource.PlayOneShot(whooshClip, whooshVolume);

        // play animation
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.ResetTrigger(attackTrigger);
            animator.SetTrigger(attackTrigger);
        }

        // start a new swing so each fist can only hit once
        if (leftFist != null) leftFist.BeginSwing();
        if (rightFist != null) rightFist.BeginSwing();

        // windup -> damage ON
        CanDealDamage = false;
        yield return new WaitForSeconds(attackWindup);

        CanDealDamage = true;
        yield return new WaitForSeconds(damageActiveTime);

        CanDealDamage = false;

        // cooldown
        nextAttackTime = Time.time + attackCooldown;

        // release slot so someone else can attack
        ReleaseSlot();

        // resume movement
        if (!dead && agent != null && agent.enabled)
            agent.isStopped = false;

        attackRoutine = null;
        if (!dead) state = State.Orbit;
    }

    void Orbit()
    {
        orbitAngle += orbitSpeedDegrees * orbitDrift * Time.deltaTime;

        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;
        Vector3 orbitPos = player.position + offset;

        SetDestinationThrottled(orbitPos);
    }

    void SetDestinationThrottled(Vector3 pos)
    {
        if (Time.time < nextRepathTime) return;
        nextRepathTime = Time.time + repathRate;

        agent.isStopped = false;
        agent.SetDestination(pos);
    }

    void FacePlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * faceTurnSpeed);
    }

    bool TryClaimSlot()
    {
        if (hasSlot) return true;

        // Keep your original anti-dogpile behavior:
        if (attackerIds.Contains(id)) { hasSlot = true; return true; }
        if (attackerIds.Count >= maxAttackers) return false;

        attackerIds.Add(id);
        hasSlot = true;
        return true;
    }

    void ReleaseSlot()
    {
        if (!hasSlot) return;
        hasSlot = false;
        attackerIds.Remove(id);
    }

    void OnDisable()
    {
        ReleaseSlot();
        CanDealDamage = false;
    }

    void OnDied(Health who, Health.DamageType type)
    {
        dead = true;
        CanDealDamage = false;

        ReleaseSlot();

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        enabled = false;
    }

    // Called by Health.cs when enemy dies
    public void OnDeath()
    {
        dead = true;
        CanDealDamage = false;

        ReleaseSlot();

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        enabled = false;
    }
}