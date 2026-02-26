using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public GameObject winScreen;
    public GameObject loseScreen;

    [Header("Enemy Tracking")]
    public int enemiesAlive;

    bool gameOver;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (winScreen) winScreen.SetActive(false);
        if (loseScreen) loseScreen.SetActive(false);

        // Auto-count enemies that have Health + Enemy tag (or just Health)
        // Best: tag all enemies as "Enemy"
        var allHealth = FindObjectsByType<Health>(FindObjectsSortMode.None);
        enemiesAlive = 0;

        foreach (var h in allHealth)
        {
            // If you want ONLY enemies counted, use a tag check:
            // if (!h.CompareTag("Enemy")) continue;

            // More robust: count anything with EnemyAI / EnemyAI_Navmesh
            if (h.GetComponent<EnemyAI>() == null && h.GetComponent<EnemyAI_Navmesh>() == null)
                continue;

            enemiesAlive++;
            h.OnDeath -= OnEnemyDied; // prevent double subscribe
            h.OnDeath += OnEnemyDied;
        }

        Debug.Log($"Enemies alive: {enemiesAlive}");
    }

    void OnEnemyDied(Health who, Health.DamageType type)
    {
        if (gameOver) return;

        enemiesAlive--;
        if (enemiesAlive <= 0)
        {
            Win();
        }
    }

    public void Lose()
    {
        if (gameOver) return;
        gameOver = true;

        if (loseScreen) loseScreen.SetActive(true);
        Debug.Log("LOSE!");
        Time.timeScale = 0f;
    }

    public void Win()
    {
        if (gameOver) return;
        gameOver = true;

        if (winScreen) winScreen.SetActive(true);
        Debug.Log("WIN!");
        Time.timeScale = 0f;
    }
}
