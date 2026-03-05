using UnityEngine;
using UnityEngine.InputSystem;

public class DebugKillPlayer : MonoBehaviour
{
    public PlayerHealth playerHealth;

    [Header("Debug Key (Input System)")]
    public Key killKey = Key.K;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
            Debug.LogWarning("[DebugKillPlayer] No PlayerHealth found in scene.");
    }

    void Update()
    {
        if (playerHealth == null) return;
        if (Keyboard.current == null) return;

        // Check chosen key
        if (Keyboard.current[killKey].wasPressedThisFrame)
        {
            Debug.Log("[DebugKillPlayer] Kill key pressed -> killing player");
            playerHealth.KillInstant();
        }
    }
}