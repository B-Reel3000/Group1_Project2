using UnityEngine;

public class EnemyActivator : MonoBehaviour
{
    EnemyAI ai;

    void Awake()
    {
        ai = GetComponent<EnemyAI>();
    }

    public void SetActiveForCombat(bool active)
    {
        // Freeze = disable AI (enemy stays in scene but won't move/attack)
        if (ai != null) ai.enabled = active;
    }
}

