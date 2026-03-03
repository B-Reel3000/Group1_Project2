using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_Navmesh : MonoBehaviour
{
    public enum State { Chase, Orbit, AttackWindup, Recover, Dead }
    public State state;

    [Header("Target")]
    public Transform player;

    [Header("NavMesh")]
    public float repathRate = 0.2f;

    [Header("Distances")]
    public float orbitEnterDistance = 5.0f;
    public float orbitExitDistance = 8.0f;
    public float attackDistance = 1.7f;

    [Header("Orbit")]
    public float orbitRadius = 4.5f;              // ✅ bigger = less crowding
    public float orbitSpeedDegrees = 70f;

    [Header("Attack")]
    public float attackWindup = 0.28f;
    public float attackCooldown = 1.35f;
    public int damage = 1;

    [Header("Animation")]
    public Animator animator;
    public string speedParam = "Speed";
    public string attackTrigger = "Punch";        // MUST exist in Animator
    public float faceTurnSpeed = 12f;

    [Header("Feel")]
    public float postHitPushBack = 0.8f;          // enemy steps back after attacking
    public float pushBackTime = 0.15f;

    NavMeshAgent agent;
    int id;

    float nextRepathTime;
    float nextAttackTime;

    float orbitAngle;
    float orbitDrift;

    bool hasSlot;
    bool attackTriggered;
    bool damageApplied;
    Coroutine stateRoutine;

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

        orbitAngle = Random.Range(0f, 360f);
        orbitDrift = Random.Range(0.8f, 1.25f);

        state = State.Chase;
    }

    void Update()
    {
        if (state == State.Dead) return;
        if (player == null) return;
        if (!agent.isOnNavMesh) return;

        float distSqr = (player.position - transform.position).sqrMagnitude;
        float orbitEnterSqr = orbitEnterDistance * orbitEnterDistance;
        float orbitExitSqr = orbitExitDistance * orbitExitDistance;
        float attackSqr = attackDistance * attackDistance;

        if (animator != null)
            animator.SetFloat(speedParam, agent.velocity.magnitude);

        switch (state)
        {
            case State.Chase:
                SetDestinationThrottled(player.position);

                if (distSqr <= orbitEnterSqr)
                    state = State.Orbit;
                break;

            case State.Orbit:
                // too far -> chase
                if (distSqr > orbitExitSqr)
                {
                    ReleaseSlot();
                    state = State.Chase;
                    break;
                }

                // try to get an attack slot
                if (Time.time >= nextAttackTime && TryClaimSlot())
                {
                    // close in to attack distance
                    if (distSqr > attackSqr)
                    {
                        SetDestinationThrottled(player.position);
                    }
                    else
                    {
                        StartAttack();
                    }
                }
                else
                {
                    Orbit();
                }
                break;

            case State.AttackWindup:
                FacePlayer();
                break;

            case State.Recover:
                FacePlayer();
                break;
        }
    }

    void StartAttack()
    {
        if (stateRoutine != null) StopCoroutine(stateRoutine);
        stateRoutine = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        state = State.AttackWindup;
        damageApplied = false;
        attackTriggered = false;

        // stop moving for the strike
        agent.isStopped = true;
        agent.ResetPath();

        // Try to trigger animation
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.ResetTrigger(attackTrigger);
            animator.SetTrigger(attackTrigger);
            attackTriggered = true;
        }

        // If trigger didn't happen (animator missing), we DO NOT deal damage.
        if (!attackTriggered)
        {
            ReleaseSlot();
            agent.isStopped = false;
            nextAttackTime = Time.time + attackCooldown;
            state = State.Orbit;
            stateRoutine = null;
            yield break;
        }

        // windup
        float t = 0f;
        while (t < attackWindup)
        {
            t += Time.deltaTime;
            FacePlayer();
            yield return null;
        }

        // ✅ Damage gate: must have slot + must pass global spacing
        if (!damageApplied && player != null && state != State.Dead)
        {
            bool canDeal = true;

            if (EnemyManager.Instance != null)
            {
                if (!EnemyManager.Instance.CanDealDamageNow())
                    canDeal = false;
            }

            float hitRange = attackDistance + 0.35f;
            float distSqr = (player.position - transform.position).sqrMagnitude;

            if (canDeal && distSqr <= hitRange * hitRange)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                    damageApplied = true;

                    if (EnemyManager.Instance != null)
                        EnemyManager.Instance.ConsumeGlobalDamageWindow();
                }
            }
        }

        // recover + backstep to reduce crowding
        state = State.Recover;

        // backstep a bit
        if (agent.enabled && agent.isOnNavMesh)
        {
            Vector3 back = (transform.position - player.position);
            back.y = 0f;
            back = back.sqrMagnitude < 0.001f ? -transform.forward : back.normalized;

            Vector3 target = transform.position + back * postHitPushBack;

            agent.isStopped = false;
            agent.SetDestination(target);

            yield return new WaitForSeconds(pushBackTime);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }

        // cooldown + release
        nextAttackTime = Time.time + attackCooldown;

        ReleaseSlot();

        if (agent != null) agent.isStopped = false;
        state = State.Orbit;
        stateRoutine = null;
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

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(pos);
        }
    }

    void FacePlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * faceTurnSpeed);
    }

    bool TryClaimSlot()
    {
        if (hasSlot) return true;

        if (EnemyManager.Instance == null)
        {
            // if no manager exists, they will all attack -> terrible
            return true;
        }

        hasSlot = EnemyManager.Instance.TryClaimSlot(id);
        return hasSlot;
    }

    void ReleaseSlot()
    {
        if (!hasSlot) return;
        hasSlot = false;

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.ReleaseSlot(id);
    }

    // Called by Health on death
    public void OnDeath()
    {
        if (state == State.Dead) return;

        state = State.Dead;

        ReleaseSlot();

        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
            stateRoutine = null;
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        enabled = false;
    }

    void OnDisable()
    {
        ReleaseSlot();
    }
}