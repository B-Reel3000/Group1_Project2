using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(Health))]
public class BossJack : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;
    public Transform shootPoint;

    [Header("Ranged Combat")]
    public float shootRange = 35f;
    public float preferredDistance = 14f;
    public float retreatDistance = 9f;
    public float shootCooldown = 0.9f;
    public int gunDamage = 1;

    [Header("Animation")]
    public string shootTrigger = "Shoot";  // create this Trigger in Animator
    public string moveSpeedParam = "Speed"; // optional
    public bool facePlayerWhileShooting = true;

    [Header("Muzzle Flash Light")]
    public Light muzzleLight;              // drag your Point Light here
    public float muzzleFlashTime = 0.05f;  // 0.03 - 0.07 feels good
    public float muzzleLightIntensity = 8f;

    [Header("Teleport On Hit")]
    public Transform[] teleportPoints;
    public float teleportCooldown = 1.25f;
    public float minDistanceFromPlayer = 7f;

    [Header("Teleport Smoke VFX")]
    public GameObject smokePrefab;
    public float smokeLifetime = 2.0f;
    public Vector3 smokeOffset = Vector3.zero;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gunshotClip;
    public AudioClip teleportClip;

    float nextShootTime;
    float nextTeleportTime;

    Health health;
    bool dead;

    Coroutine muzzleRoutine;

    void Awake()
    {
        health = GetComponent<Health>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Make sure muzzle light starts OFF
        if (muzzleLight != null)
            muzzleLight.enabled = false;
    }

    void Start()
    {
        if (player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc) player = pc.transform;
        }

        // Boss should not be one-shot by gun
        health.gunOneHitKill = false;

        health.OnDamaged += OnDamaged;
        health.OnDeath += OnDeath;
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged -= OnDamaged;
            health.OnDeath -= OnDeath;
        }
    }

    void Update()
    {
        if (dead) return;
        if (player == null) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.position);

        HandleMovement(dist);

        if (facePlayerWhileShooting)
            FacePlayer();

        // Optional animator speed
        if (animator != null && !string.IsNullOrEmpty(moveSpeedParam))
            animator.SetFloat(moveSpeedParam, agent.velocity.magnitude);

        // Shoot
        if (Time.time >= nextShootTime && dist <= shootRange && dist >= 6f)
        {
            nextShootTime = Time.time + shootCooldown;
            Shoot();
        }
    }

    void HandleMovement(float dist)
    {
        if (dist < retreatDistance)
        {
            Vector3 away = (transform.position - player.position);
            away.y = 0f;
            away = away.sqrMagnitude < 0.001f ? transform.forward : away.normalized;

            Vector3 target = transform.position + away * 6f;
            agent.SetDestination(target);
        }
        else if (dist > preferredDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.ResetPath();
        }
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
    }

    void Shoot()
    {
        if (shootPoint == null) return;

        // Play shoot animation
        if (animator != null && !string.IsNullOrEmpty(shootTrigger))
        {
            animator.ResetTrigger(shootTrigger);
            animator.SetTrigger(shootTrigger);
        }

        // Muzzle flash light
        DoMuzzleFlash();

        // Gunshot sound
        if (audioSource != null && gunshotClip != null)
            audioSource.PlayOneShot(gunshotClip);

        // Hitscan ray
        Ray ray = new Ray(shootPoint.position, shootPoint.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, shootRange, ~0, QueryTriggerInteraction.Ignore))
        {
            PlayerHealth ph = hit.collider.GetComponentInParent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(gunDamage);
        }
    }

    void DoMuzzleFlash()
    {
        if (muzzleLight == null) return;

        if (muzzleRoutine != null)
            StopCoroutine(muzzleRoutine);

        muzzleRoutine = StartCoroutine(MuzzleFlashRoutine());
    }

    IEnumerator MuzzleFlashRoutine()
    {
        float originalIntensity = muzzleLight.intensity;

        muzzleLight.intensity = muzzleLightIntensity;
        muzzleLight.enabled = true;

        yield return new WaitForSeconds(muzzleFlashTime);

        muzzleLight.enabled = false;
        muzzleLight.intensity = originalIntensity;

        muzzleRoutine = null;
    }

    void OnDamaged(Health who, Health.DamageType type, int amount)
    {
        if (dead) return;
        if (Time.time < nextTeleportTime) return;

        nextTeleportTime = Time.time + teleportCooldown;
        TeleportWithSmoke();
    }

    void TeleportWithSmoke()
    {
        if (teleportPoints == null || teleportPoints.Length == 0) return;

        Vector3 fromPos = transform.position;

        Transform target = null;
        for (int i = 0; i < teleportPoints.Length; i++)
        {
            Transform p = teleportPoints[Random.Range(0, teleportPoints.Length)];
            if (p == null) continue;

            if (player == null || Vector3.Distance(p.position, player.position) >= minDistanceFromPlayer)
            {
                target = p;
                break;
            }
        }

        if (target == null)
            target = teleportPoints[Random.Range(0, teleportPoints.Length)];

        if (target == null) return;

        Vector3 toPos = target.position;

        SpawnSmoke(fromPos);

        if (audioSource != null && teleportClip != null)
            audioSource.PlayOneShot(teleportClip);

        agent.Warp(toPos);
        agent.ResetPath();

        SpawnSmoke(toPos);
    }

    void SpawnSmoke(Vector3 worldPos)
    {
        if (smokePrefab == null) return;

        GameObject vfx = Instantiate(smokePrefab, worldPos + smokeOffset, Quaternion.identity);
        if (smokeLifetime > 0.01f)
            Destroy(vfx, smokeLifetime);
    }

    void OnDeath(Health who, Health.DamageType type)
    {
        dead = true;

        if (muzzleLight != null)
            muzzleLight.enabled = false;

        if (muzzleRoutine != null)
        {
            StopCoroutine(muzzleRoutine);
            muzzleRoutine = null;
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }
    }
}