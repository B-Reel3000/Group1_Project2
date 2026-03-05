using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject gameplayCanvas;   // HUD (health/ammo/reticle canvas)
    public GameObject winPanel;
    public GameObject losePanel;

    bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ---------- WIN ----------
    public void Win()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("PLAYER WIN");

        // IMPORTANT: do not freeze time (fade + anims need to play)
        Time.timeScale = 1f;

        if (gameplayCanvas != null)
            gameplayCanvas.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ---------- LOSE ----------
    public void Lose()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("PLAYER LOSE");

        // IMPORTANT: do not freeze time (fade + anims need to play)
        Time.timeScale = 1f;

        if (gameplayCanvas != null)
            gameplayCanvas.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}