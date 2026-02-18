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

    int currentWaveIndex = -1;
    int aliveInWave = 0;

    void Start()
    {
        // Turn everything off first
        foreach (var w in waves)
        {
            foreach (var e in w.enemies)
            {
                if (e == null) continue;
                e.SetActive(false);
            }
        }

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

            // Activate enemy
            e.SetActive(true);
            aliveInWave++;

            // Subscribe to death
            Health h = e.GetComponent<Health>();
            if (h == null) h = e.GetComponentInChildren<Health>();

            if (h != null)
            {
                h.OnDeath -= OnEnemyDied; // prevent double-subscribe
                h.OnDeath += OnEnemyDied;
            }
            else
            {
                Debug.LogWarning($"Wave enemy {e.name} has no Health component.");
            }
        }

        Debug.Log($"Wave {waveIndex + 1} started with {aliveInWave} enemies.");
    }

    void OnEnemyDied(Health whoDied, Health.DamageType type)
    {
        aliveInWave--;

        if (aliveInWave <= 0)
        {
            int next = currentWaveIndex + 1;
            if (next < waves.Count)
            {
                StartWave(next);
            }
            else
            {
                Debug.Log("All waves cleared!");
            }
        }
    }
}
