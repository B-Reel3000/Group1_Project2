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

    [Header("Combat")]
    public float shootRange = 30f;
    public float shootCooldown = 1.2f;
    public int gunDamage = 1;

    [Header("Teleport")]
    public Transform[] teleportPoints;
    public float teleportCooldown = 1.5f;

    [Header("Effects")]
    public GameObject smokePrefab;
    public float smokeLife = 2f;
    public Light muzzleLight;
    public float muzzleFlashTime = 0.05f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gunshotClip;
    public AudioClip teleportClip;

    float nextShot;
    float nextTeleport;

    Health health;
    bool dead;

    void Awake()
    {
        health = GetComponent<Health>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

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

        health.gunOneHitKill = false;

        health.OnDamaged += OnDamaged;
        health.OnDeath += OnDeath;

        BossIntroUI intro = FindFirstObjectByType<BossIntroUI>();
        if (intro != null)
            intro.ShowBossName("Jack 'The Devil' Porter");
    }

    void Update()
    {
        if (dead) return;
        if (player == null) return;

        FacePlayer();

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= shootRange && Time.time >= nextShot)
        {
            nextShot = Time.time + shootCooldown;
            Shoot();
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
            Time.deltaTime * 8f
        );
    }

    void Shoot()
    {
        if (animator != null)
            animator.SetTrigger("Shoot");

        if (audioSource != null && gunshotClip != null)
            audioSource.PlayOneShot(gunshotClip);

        if (muzzleLight != null)
            StartCoroutine(MuzzleFlash());

        Ray ray = new Ray(shootPoint.position, shootPoint.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, shootRange))
        {
            PlayerHealth ph = hit.collider.GetComponentInParent<PlayerHealth>();

            if (ph != null)
                ph.TakeDamage(gunDamage);
        }
    }

    IEnumerator MuzzleFlash()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(muzzleFlashTime);
        muzzleLight.enabled = false;
    }

    void OnDamaged(Health h, Health.DamageType type, int amount)
    {
        if (dead) return;
        if (Time.time < nextTeleport) return;

        nextTeleport = Time.time + teleportCooldown;

        Teleport();
    }

    void Teleport()
    {
        if (teleportPoints.Length == 0) return;

        Vector3 startPos = transform.position;

        Transform target = teleportPoints[Random.Range(0, teleportPoints.Length)];

        if (target == null) return;

        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, startPos, Quaternion.identity);
            Destroy(smoke, smokeLife);
        }

        if (audioSource != null && teleportClip != null)
            audioSource.PlayOneShot(teleportClip);

        agent.Warp(target.position);

        if (smokePrefab != null)
        {
            GameObject smoke = Instantiate(smokePrefab, target.position, Quaternion.identity);
            Destroy(smoke, smokeLife);
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