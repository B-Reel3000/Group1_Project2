using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string name = "Wave";
        public GameObject[] enemies; // drag enemies here in Inspector
    }

    [Header("Waves")]
    public Wave[] waves;

    int currentWaveIndex = -1;
    int aliveInWave = 0;

    void Start()
    {
        // Disable all enemies at start so they don't all rush you
        for (int w = 0; w < waves.Length; w++)
        {
            if (waves[w].enemies == null) continue;

            for (int i = 0; i < waves[w].enemies.Length; i++)
            {
                if (waves[w].enemies[i] != null)
                    waves[w].enemies[i].SetActive(false);
            }
        }

        StartNextWave();
    }

    void StartNextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            // ✅ All waves finished
            if (GameManager.Instance != null)
                GameManager.Instance.Win();
            return;
        }

        Wave wave = waves[currentWaveIndex];
        aliveInWave = 0;

        if (wave.enemies == null || wave.enemies.Length == 0)
        {
            // empty wave -> skip
            StartNextWave();
            return;
        }

        // Enable wave enemies and subscribe to their death
        for (int i = 0; i < wave.enemies.Length; i++)
        {
            GameObject e = wave.enemies[i];
            if (e == null) continue;

            e.SetActive(true);

            Health h = e.GetComponent<Health>();
            if (h != null)
            {
                // prevent double subscriptions if you restart/play again in editor
                h.OnDeath -= OnEnemyDied;
                h.OnDeath += OnEnemyDied;

                aliveInWave++;
            }
            else
            {
                // If no Health, it can't be counted. Still allow it to exist.
                Debug.LogWarning($"WaveManager: Enemy '{e.name}' has no Health component, so it won't count toward wave completion.");
            }
        }

        Debug.Log($"Wave {currentWaveIndex + 1} started with {aliveInWave} enemies.");
        if (aliveInWave <= 0)
        {
            // Nothing countable -> skip
            StartNextWave();
        }
    }

    void OnEnemyDied(Health who, Health.DamageType type)
    {
        // Only count deaths from the active wave
        aliveInWave--;
        if (aliveInWave < 0) aliveInWave = 0;

        // Unsubscribe to be safe
        who.OnDeath -= OnEnemyDied;

        if (aliveInWave == 0)
        {
            // Wave cleared -> next wave
            StartNextWave();
        }
    }
}