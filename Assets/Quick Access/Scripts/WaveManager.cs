using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public List<GameObject> enemies = new List<GameObject>();
    }

    public List<Wave> waves = new List<Wave>();

    int currentWaveIndex = 0;
    int aliveInWave = 0;

    void Start()
    {
        // Freeze ALL enemies first
        foreach (var w in waves)
        {
            foreach (var e in w.enemies)
            {
                if (e == null) continue;
                SetEnemyActive(e, false);
            }
        }

        // Start wave 0
        StartWave(0);
    }

    void StartWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Count) return;

        currentWaveIndex = waveIndex;
        aliveInWave = 0;

        foreach (var e in waves[waveIndex].enemies)
        {
            if (e == null) continue;

            SetEnemyActive(e, true);
            aliveInWave++;

            // Subscribe to death
            Health h = e.GetComponentInParent<Health>() ?? e.GetComponent<Health>();
            if (h != null)
            {
                h.OnDeath -= OnEnemyDied; // prevent double subscribe
                h.OnDeath += OnEnemyDied;
            }
        }

        Debug.Log($"Wave {waveIndex} started with {aliveInWave} enemies.");
    }

    void OnEnemyDied(Health whoDied, Health.DamageType type)
    {
        aliveInWave--;

        if (aliveInWave <= 0)
        {
            int next = currentWaveIndex + 1;
            if (next < waves.Count)
                StartWave(next);
            else
                Debug.Log("All waves cleared!");
        }
    }

    void SetEnemyActive(GameObject enemy, bool active)
    {
        // If you prefer: enemy.SetActive(active);
        EnemyActivator act = enemy.GetComponent<EnemyActivator>();
        if (act != null) act.SetActiveForCombat(active);

        // Optional: also stop them from “bumping” if frozen
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = !active;
    }
}

