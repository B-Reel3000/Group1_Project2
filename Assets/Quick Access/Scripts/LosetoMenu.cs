using UnityEngine;
using System.Collections;

public class LoseToMenu : MonoBehaviour
{
    [Header("Player Health")]
    public PlayerHealth playerHealth;

    [Header("Scene")]
    public string mainMenuScene = "MainMenu";

    [Header("Timing")]
    public float delayBeforeFade = 1.2f;   // let death anim start
    public float forceFadeFallbackDelay = 0.1f;

    bool triggered;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    void Update()
    {
        if (triggered) return;
        if (playerHealth == null) return;

        if (playerHealth.currentHealth <= 0)
        {
            triggered = true;
            StartCoroutine(LoseRoutine());
        }
    }

    IEnumerator LoseRoutine()
    {
        // make sure time is running so animator can play
        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(delayBeforeFade);

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeToScene(mainMenuScene);
        }
        else
        {
            // No FadeManager in this run -> hard load
            yield return new WaitForSecondsRealtime(forceFadeFallbackDelay);
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }
    }
}