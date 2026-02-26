using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGun : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public Camera cam;                     // assign Main Camera in inspector
    public Transform muzzle;               // optional (for VFX later)

    [Header("Gun")]
    public int maxAmmo = 6;
    public int currentAmmo = 6;
    public int damage = 999;               // one-hit default
    public float fireRate = 0.2f;
    public float range = 100f;
    public LayerMask hitLayers = ~0;

    [Header("Debug")]
    public bool debugLogs = true;
    public bool drawRay = true;

    // >>> HUD COMPATIBILITY (fixes your error)
    public int ammo => currentAmmo;
    public int maxAmmoCount => maxAmmo;

    float nextFireTime;

    void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (controller == null) return;
        if (!controller.IsAiming) return;          // only shoot in gun mode
        if (Time.time < nextFireTime) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (currentAmmo <= 0)
        {
            if (debugLogs) Debug.Log("[GUN] Click! No ammo.");
            return;
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate;

        // Shoot from camera center
        Vector3 origin = cam != null ? cam.transform.position : transform.position + Vector3.up * 1.5f;
        Vector3 dir = cam != null ? cam.transform.forward : transform.forward;

        if (drawRay)
            Debug.DrawRay(origin, dir * Mathf.Min(range, 25f), Color.red, 0.25f);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, range, hitLayers, QueryTriggerInteraction.Ignore))
        {
            if (debugLogs) Debug.Log($"[GUN] Hit: {hit.collider.name}");

            // ignore self
            if (hit.collider.GetComponentInParent<PlayerController>() != null || hit.collider.transform.IsChildOf(transform))
                return;

            Health h = hit.collider.GetComponentInParent<Health>();
            if (h != null)
            {
                if (debugLogs) Debug.Log($"[GUN] Damage applied to {h.gameObject.name}");
                h.TakeDamage(damage, Health.DamageType.Bullet);
            }
            else
            {
                if (debugLogs) Debug.Log("[GUN] Object has no Health.");
            }
        }
        else
        {
            if (debugLogs) Debug.Log("[GUN] Miss.");
        }
    }

    // used by ammo pickups
    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        if (debugLogs) Debug.Log($"[GUN] Ammo: {currentAmmo}/{maxAmmo}");
    }
}