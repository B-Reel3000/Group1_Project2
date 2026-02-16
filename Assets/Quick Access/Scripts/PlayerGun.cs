using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGun : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;  // Drag your PlayerController here
    public Camera mainCamera;            // Drag MAIN CAMERA here (the one with the Camera component)

    [Header("Revolver")]
    public int maxAmmo = 6;
    public int ammo = 6;

    [Header("Shot")]
    public float range = 100f;
    public int damage = 999;             // One-hit kill for now

    [Header("Debug")]
    public bool debugLogs = true;

    void Update()
    {
        if (controller == null || mainCamera == null) return;

        // Only shoot while aiming
        if (!controller.IsAiming) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Shoot();
    }

    void Shoot()
    {
        if (ammo <= 0)
        {
            if (debugLogs) Debug.Log("Click (no ammo)");
            return;
        }

        ammo--;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (debugLogs) Debug.Log("Hit: " + hit.collider.name);

            Health h = hit.collider.GetComponentInParent<Health>();
            if (h != null)
            {
                if (debugLogs) Debug.Log("Health found on: " + h.gameObject.name + " -> applying damage " + damage);
                h.TakeDamage(damage, Health.DamageType.Gun);
            }
            else
            {
                if (debugLogs) Debug.Log("No Health component found on hit object or its parents.");
            }
        }
        else
        {
            if (debugLogs) Debug.Log("No hit (raycast missed).");
        }
    }

    public void AddAmmo(int amount)
    {
        ammo = Mathf.Clamp(ammo + amount, 0, maxAmmo);
        if (debugLogs) Debug.Log("Ammo now: " + ammo + "/" + maxAmmo);
    }
}
