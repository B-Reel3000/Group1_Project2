using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum State { Chase, Orbit, Attack, Recover }
    public State state;

    [Header("Target")]
    public Transform player; // drag Player here OR it will auto-find PlayerController at runtime

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float turnSpeed = 540f;

    [Header("Distances")]
    [Tooltip("When closer than this, stop chasing and start orbiting.")]
    public float orbitEnterDistance = 4.0f;

    [Tooltip("When farther than this, stop orbiting and chase again. MUST be > orbitEnterDistance.")]
    public float orbitExitDistance = 6.0f;

    [Tooltip("How close the enemy must be to begin an attack step-in.")]
    public float attackDistance = 1.6f;

    [Header("Orbit")]
    public float orbitRadius = 3.5f;
    public float orbitSpeed = 1.0f; // how fast they move around the circle

    [Header("Attack")]
    public float attackWindup = 0.25f;
    public float attackCooldown = 1.25f;
    public int damage = 1;

    [Header("Anti-Dogpile: Attack Slots")]
    [Range(1, 4)]
    public int maxAttackers = 1;

    [Header("Anti-Dogpile: Personal Space")]
    public float separationRadius = 2.2f;
    public float separationStrength = 6.0f;
    public LayerMask enemyLayers; // Put enemies on "Enemy" layer and assign it here

    [Header("Anti-Thrash")]
    public float stateMinTime = 0.6f;

    [Header("Anti-Jitter (Separation Smoothing)")]
    [Tooltip("How quickly the separation force adapts. Higher = snappier, lower = smoother.")]
    public float separationSmooth = 10f;

    [Tooltip("Max strength the separation can contribute to movement direction (prevents ping-pong).")]
    public float separationMaxInfluence = 0.75f;

    [Tooltip("Ignore tiny pushes (prevents micro-jitter when already spaced).")]
    public float separationDeadZone = 0.02f;

    [Header("Debug (optional)")]
    public bool debugLogs = false;

    int id;
    float nextAttackTime;
    float stateLockUntil;

    float orbitAngle;
    float orbitDrift;

    Vector3 separationVel; // smoothed separation force

    // Local attack-slot system (no manager needed)
    static readonly System.Collections.Generic.HashSet<int> attackerIds =
        new System.Collections.Generic.HashSet<int>();

    void Start()
    {
        id = GetInstanceID();

        // Auto-find player if not assigned
        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc) player = pc.transform;
        }

        orbitAngle = Random.Range(0f, 360f);
        orbitDrift = Random.Range(0.7f, 1.3f);

        state = State.Chase;
        LockState();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Chase:
                FacePlayer();
                MoveToward(player.position);

                if (dist <= orbitEnterDistance && CanChangeState())
                {
                    state = State.Orbit;
                    LockState();
                }
                break;

            case State.Orbit:
                if (dist > orbitExitDistance && CanChangeState())
                {
                    ReleaseSlot();
                    state = State.Chase;
                    LockState();
                    break;
                }

                // Try to get an attack slot (only a few can attack)
                if (Time.time >= nextAttackTime && dist <= orbitEnterDistance && CanChangeState())
                {
                    if (TryClaimSlot())
                    {
                        state = State.Attack;
                        LockState();
                        break;
                    }
                }

                Orbit();
                break;

            case State.Attack:
                if (dist > orbitExitDistance)
                {
                    ReleaseSlot();
                    state = State.Chase;
                    LockState();
                    break;
                }

                FacePlayer();

                if (dist > attackDistance)
                {
                    MoveToward(player.position);
                }
                else
                {
                    // Do one attack, then recover/orbit
                    StartCoroutine(DoAttack());
                    state = State.Recover;
                    LockState();
                }
                break;

            case State.Recover:
                // waiting for coroutine
                break;
        }
    }

    IEnumerator DoAttack()
    {
        yield return new WaitForSeconds(attackWindup);

        // DAMAGE THE PLAYER HERE
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);

            // Small buffer so it still hits if they're barely in range
            if (dist <= attackDistance + 0.35f)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                    if (debugLogs) Debug.Log($"{name} hit player for {damage}!");
                }
                else if (debugLogs)
                {
                    Debug.LogWarning($"{name} tried to damage player, but PlayerHealth not found.");
                }
            }
            else if (debugLogs)
            {
                Debug.Log($"{name} attack whiffed (out of range).");
            }
        }

        nextAttackTime = Time.time + attackCooldown;

        ReleaseSlot();
        state = State.Orbit;
        LockState();
    }

    // -------- Movement helpers --------

    void MoveToward(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        // Compute + smooth separation so it doesn't ping-pong
        Vector3 sepTarget = ComputeSeparation();
        separationVel = Vector3.Lerp(separationVel, sepTarget, separationSmooth * Time.deltaTime);

        // Cap separation influence (prevents violent vibration)
        Vector3 sepCapped = Vector3.ClampMagnitude(separationVel, separationMaxInfluence);

        Vector3 final = dir + sepCapped;
        final.y = 0f;

        if (final.sqrMagnitude < 0.0001f)
            final = dir;

        final.Normalize();
        transform.position += final * moveSpeed * Time.deltaTime;
    }

    void FacePlayer()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    void Orbit()
    {
        // Unique orbit per enemy, with drift to avoid syncing/clumping
        orbitAngle += orbitSpeed * 60f * orbitDrift * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;
        Vector3 orbitPos = player.position + offset;

        FacePlayer();
        MoveToward(orbitPos);
    }

    Vector3 ComputeSeparation()
    {
        // If no layer mask selected, do nothing
        if (enemyLayers.value == 0) return Vector3.zero;

        Collider[] nearby = Physics.OverlapSphere(transform.position, separationRadius, enemyLayers);
        if (nearby == null || nearby.Length == 0) return Vector3.zero;

        Vector3 push = Vector3.zero;
        int count = 0;

        foreach (var col in nearby)
        {
            if (col == null) continue;

            Transform t = col.transform;

            // skip self (handles children)
            if (t == transform || t.IsChildOf(transform)) continue;

            Vector3 away = transform.position - t.position;
            away.y = 0f;

            float dist = away.magnitude;
            if (dist < 0.001f) continue;

            // stronger when close
            push += away.normalized / dist;
            count++;
        }

        if (count == 0) return Vector3.zero;

        push /= count;

        // dead zone prevents micro jitter
        if (push.sqrMagnitude < separationDeadZone)
            return Vector3.zero;

        return push.normalized * separationStrength;
    }

    // -------- Attack slot helpers --------

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

    // -------- State anti-thrash --------

    bool CanChangeState() => Time.time >= stateLockUntil;
    void LockState() => stateLockUntil = Time.time + stateMinTime;

    // Optional: visualize in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.position, orbitEnterDistance);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position, orbitExitDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, attackDistance);
        }
    }
}
