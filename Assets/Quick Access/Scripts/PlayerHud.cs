using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerController controller;
    public PlayerGun gun;
    public PlayerHealth health;

    [Header("UI Objects")]
    public GameObject ammoRoot;   // object holding ammo text (can be parent panel)
    public TMP_Text ammoText;
    public TMP_Text healthText;

    void Update()
    {
        if (health == null) return;

        // ----- HEALTH (ALWAYS VISIBLE) -----
        if (healthText != null)
            healthText.text = $"HP: {health.currentHealth}/{health.maxHealth}";

        // ----- AMMO (ONLY WHILE AIMING) -----
        bool aiming = controller != null && controller.IsAiming;

        if (ammoRoot != null)
            ammoRoot.SetActive(aiming);

        if (aiming && gun != null && ammoText != null)
            ammoText.text = $"AMMO: {gun.ammo}/{gun.maxAmmo}";
    }
}

