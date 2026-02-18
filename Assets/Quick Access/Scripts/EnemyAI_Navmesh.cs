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
    public float attackWindup = 0.25f;
    public float attackCooldown = 1.25f;
    public int damage = 1;

    [Header("Anti-Dogpile")]
    [Range(1, 4)] public int maxAttackers = 1;

    NavMeshAgent agent;
    float nextAttackTime;
    float nextRepathTime;

    int id;
    float orbitAngle;
    float orbitDrift;

    static readonly System.Collections.Generic.HashSet<int> attackerIds =
        new System.Collections.Generic.HashSet<int>();

    void Start()
    {
        id = GetInstanceID();
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc) player = pc.transform;
        }

        orbitAngle = Random.Range(0f, 360f);
        orbitDrift = Random.Range(0.7f, 1.3f);

        state = State.Chase;
    }

    void Update()
    {
        if (player == null) return;
        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.position);

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

                if (dist > attackDistance)
                {
                    SetDestinationThrottled(player.position);
                }
                else
                {
                    agent.ResetPath();
                    StartCoroutine(DoAttack());
                    state = State.Recover;
                }
                break;

            case State.Recover:
                break;
        }
    }

    IEnumerator DoAttack()
    {
        yield return new WaitForSeconds(attackWindup);

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackDistance + 0.35f)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage);
            }
        }

        nextAttackTime = Time.time + attackCooldown;

        ReleaseSlot();
        state = State.Orbit;
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

        agent.SetDestination(pos);
    }

    bool TryClaimSlot()
    {
        if (attackerIds.Contains(id)) return true;
        if (attackerIds.Count >= maxAttackers) return false;

        attackerIds.Add(id);
        return true;
    }

    void ReleaseSlot()
    {
        attackerIds.Remove(id);
    }

    void OnDisable()
    {
        ReleaseSlot();
    }
}
