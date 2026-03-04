using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossFloyd : MonoBehaviour
{
    [Header("Floyd = Regular Dude With More HP")]
    public int bossMaxHealth = 25;

    void Start()
    {
        var h = GetComponent<Health>();
        h.gunOneHitKill = false;         // bosses should not be 1-shot by gun
        h.maxHealth = bossMaxHealth;
        h.currentHealth = bossMaxHealth;
    }
}