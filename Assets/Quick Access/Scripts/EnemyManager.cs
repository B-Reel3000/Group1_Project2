using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Anti-Dogpile")]
    [Range(1, 6)] public int maxAttackers = 1;

    [Tooltip("Minimum time between ANY enemy damage attempts.")]
    public float globalDamageSpacing = 0.35f;

    HashSet<int> attackerIds = new HashSet<int>();
    float nextGlobalDamageTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        nextGlobalDamageTime = 0f;
    }

    public bool TryClaimSlot(int enemyId)
    {
        if (attackerIds.Contains(enemyId)) return true;
        if (attackerIds.Count >= maxAttackers) return false;

        attackerIds.Add(enemyId);
        return true;
    }

    public void ReleaseSlot(int enemyId)
    {
        attackerIds.Remove(enemyId);
    }

    public bool CanDealDamageNow()
    {
        return Time.time >= nextGlobalDamageTime;
    }

    public void ConsumeGlobalDamageWindow()
    {
        nextGlobalDamageTime = Time.time + globalDamageSpacing;
    }

    public int CurrentAttackers => attackerIds.Count;
}