using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Range(1, 4)]
    public int maxAttackers = 2;

    HashSet<int> attackerIds = new HashSet<int>();

    void Awake()
    {
        Instance = this;
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

    public int CurrentAttackers => attackerIds.Count;
}