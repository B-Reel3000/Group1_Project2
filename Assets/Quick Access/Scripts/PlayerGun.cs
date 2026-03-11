using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerGun : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public Transform shootPoint;

    [Header("Ammo")]
    public int maxAmmo = 6;
    public int ammo = 6;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gunshotClip;

    [Header("Muzzle Flash Light")]
    public Light muzzleLight;
    public float muzzleIntensity = 12f;
    public float muzzleDuration = 0.045f;

    [Header("Muzzle Particle")]
    public ParticleSystem muzzleParticle;

    [Header("Shoot Settings")]
    public float fireCooldown = 0.25f;
    public float range = 100f;

    [Header("Debug")]
    public bool debugRay = true;

    float nextFireTime;
    Coroutine muzzleRoutine;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (muzzleLight == null && shootPoint != null)
            muzzleLight = shootPoint.GetComponentInChildren<Light>(true);

        if (muzzleParticle == null && shootPoint != null)
            muzzleParticle = shootPoint.GetComponentInChildren<ParticleSystem>(true);

        if (muzzleLight != null)
        {
            muzzleLight.enabled = true;
            muzzleLight.intensity = 0f;
        }
    }

    void Update()
    {
        if (controller == null || !controller.IsAiming) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            if (ammo <= 0)
            {
                Debug.Log("Out of ammo!");
                return;
            }

            nextFireTime = Time.time + fireCooldown;
            ammo--;

            Shoot();
        }
    }

    void Shoot()
    {
        if (audioSource != null && gunshotClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(gunshotClip);
        }

        if (muzzleLight != null)
        {
            if (muzzleRoutine != null)
                StopCoroutine(muzzleRoutine);

            muzzleRoutine = StartCoroutine(MuzzleFlash());
        }

        if (muzzleParticle != null)
            muzzleParticle.Play();

        if (shootPoint == null)
        {
            Debug.LogWarning("PlayerGun: shootPoint not assigned.");
            return;
        }

        Ray ray = new Ray(shootPoint.position, shootPoint.forward);

        if (debugRay)
            Debug.DrawRay(ray.origin, ray.direction * range, Color.yellow, 0.5f);

        if (Physics.Raycast(ray, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore))
        {
            Health h = hit.collider.GetComponentInParent<Health>();
            if (h != null)
                h.TakeDamage(1, Health.DamageType.Gun);
        }
    }

    IEnumerator MuzzleFlash()
    {
        muzzleLight.intensity = muzzleIntensity * Random.Range(0.85f, 1.15f);
        yield return new WaitForSecondsRealtime(muzzleDuration);
        muzzleLight.intensity = 0f;
    }

    public void AddAmmo(int amount)
    {
        if (amount <= 0) return;
        ammo = Mathf.Clamp(ammo + amount, 0, maxAmmo);
    }
}