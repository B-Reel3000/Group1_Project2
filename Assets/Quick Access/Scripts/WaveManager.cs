using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string name = "Wave";
        public GameObject[] enemies; // assign in Inspector
    }

    [Header("Waves")]
    public Wave[] waves;

    [Header("End-of-Level Flow")]
    public float winDelay = 3.0f;                 // time to let last KO animation play
    public string nextSceneAfterWaves = "MainMenu";

    [Header("Safety")]
    public bool requireAtLeastOneCountedEnemy = true; // prevents instant end if setup is wrong

    int currentWaveIndex = -1;
    int aliveInWave = 0;
    bool ending = false;
    Coroutine endRoutine;

    void Awake()
    {
        // ✅ If this WaveManager somehow survived via DontDestroyOnLoad, kill it.
        // This prevents Level 1's coroutine from firing during Level 2.
        if (gameObject.scene.name == "DontDestroyOnLoad")
        {
            Debug.LogWarning("[WaveManager] Found in DontDestroyOnLoad. Destroying to prevent cross-scene triggers.");
            Destroy(gameObject);
            return;
        }
    }

    void OnDisable()
    {
        if (endRoutine != null) StopCoroutine(endRoutine);
        endRoutine = null;
        ending = false;
    }

    void Start()
    {
        // Disable all enemies initially so only the current wave is active
        for (int w = 0; w < waves.Length; w++)
        {
            if (waves[w].enemies == null) continue;

            for (int i = 0; i < waves[w].enemies.Length; i++)
                if (waves[w].enemies[i] != null)
                    waves[w].enemies[i].SetActive(false);
        }

        StartNextWave();
    }

    void StartNextWave()
    {
        currentWaveIndex++;

        // All waves complete -> end sequence
        if (currentWaveIndex >= waves.Length)
        {
            // Safety: don't instantly end if nothing was ever counted
            if (requireAtLeastOneCountedEnemy)
            {
                Debug.LogWarning("[WaveManager] No more waves. If this is happening at level start, check Wave setup + Health on enemies.");
            }

            endRoutine = StartCoroutine(EndLevelSequence());
            return;
        }

        Wave wave = waves[currentWaveIndex];
        aliveInWave = 0;

        if (wave.enemies == null || wave.enemies.Length == 0)
        {
            Debug.LogWarning($"[WaveManager] Wave {currentWaveIndex + 1} has no enemies assigned. Skipping.");
            StartNextWave();
            return;
        }

        for (int i = 0; i < wave.enemies.Length; i++)
        {
            GameObject e = wave.enemies[i];
            if (e == null) continue;

            e.SetActive(true);

            Health h = e.GetComponent<Health>();
            if (h != null)
            {
                h.OnDeath -= OnEnemyDied;
                h.OnDeath += OnEnemyDied;
                aliveInWave++;
            }
            else
            {
                Debug.LogWarning($"[WaveManager] Enemy '{e.name}' has no Health on the ROOT object you assigned. It will not count.");
            }
        }

        Debug.Log($"[WaveManager] Wave {currentWaveIndex + 1} started with {aliveInWave} enemies.");

        // Safety: if nothing counted, don't auto-win unless you want that behavior
        if (aliveInWave <= 0)
        {
            if (requireAtLeastOneCountedEnemy)
            {
                Debug.LogWarning("[WaveManager] Wave has 0 countable enemies. NOT ending. Fix Health hookup or enemy assignment.");
                return;
            }
            else
            {
                StartNextWave();
            }
        }
    }

    void OnEnemyDied(Health who, Health.DamageType type)
    {
        if (ending) return;

        aliveInWave--;
        if (aliveInWave < 0) aliveInWave = 0;

        if (who != null)
            who.OnDeath -= OnEnemyDied;

        if (aliveInWave == 0)
        {
            if (currentWaveIndex >= waves.Length - 1)
                endRoutine = StartCoroutine(EndLevelSequence());
            else
                StartNextWave();
        }
    }

    IEnumerator EndLevelSequence()
    {
        if (ending) yield break;
        ending = true;

        Debug.Log($"[WaveManager] End in {winDelay}s -> '{nextSceneAfterWaves}' (Scene: {SceneManager.GetActiveScene().name})");

        yield return new WaitForSeconds(winDelay);

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene(nextSceneAfterWaves);
        else
            SceneManager.LoadScene(nextSceneAfterWaves);
    }
}