using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("UI")]
    public CanvasGroup fadeGroup; // drag the Panel's CanvasGroup here

    [Header("Timing")]
    public float fadeOutTime = 0.6f;
    public float fadeInTime = 0.6f;

    [Header("Auto Fade-In On Scene Load")]
    public bool autoFadeInOnSceneLoad = true;

    [Tooltip("Scenes that should NOT auto fade-in (ex: MainMenu). Leave empty if you want fade-in everywhere.")]
    public List<string> autoFadeInExcludeScenes = new List<string> { "MainMenu" };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!autoFadeInOnSceneLoad) return;
        if (fadeGroup == null) return;

        if (autoFadeInExcludeScenes != null && autoFadeInExcludeScenes.Contains(scene.name))
            return;

        // Start black then fade in
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;
        StartCoroutine(Fade(0f, fadeInTime));
    }

    // --- Existing style API ---
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeToSceneRoutine(sceneName));
    }

    IEnumerator FadeToSceneRoutine(string sceneName)
    {
        Time.timeScale = 1f;

        yield return Fade(1f, fadeOutTime);

        SceneManager.LoadScene(sceneName);

        // wait 1 frame so scene initializes
        yield return null;

        // If autoFadeInOnSceneLoad is enabled, OnSceneLoaded will handle fade-in.
        // If disabled, do it here:
        if (!autoFadeInOnSceneLoad)
            yield return Fade(0f, fadeInTime);
    }

    public void FadeInNow(float duration = -1f)
    {
        if (fadeGroup == null) return;
        if (duration < 0f) duration = fadeInTime;

        StopAllCoroutines();
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;
        StartCoroutine(Fade(0f, duration));
    }

    public void FadeOutNow(float duration = -1f)
    {
        if (fadeGroup == null) return;
        if (duration < 0f) duration = fadeOutTime;

        StopAllCoroutines();
        fadeGroup.blocksRaycasts = true;
        StartCoroutine(Fade(1f, duration));
    }

    IEnumerator Fade(float target, float duration)
    {
        if (fadeGroup == null) yield break;

        float start = fadeGroup.alpha;
        float t = 0f;

        fadeGroup.blocksRaycasts = true;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = duration <= 0.0001f ? 1f : t / duration;
            fadeGroup.alpha = Mathf.Lerp(start, target, p);
            yield return null;
        }

        fadeGroup.alpha = target;
        fadeGroup.blocksRaycasts = (target > 0.001f);
    }

    // Convenience helper if you want one-liners elsewhere
    public static void FadeToSceneSafe(string sceneName)
    {
        if (Instance != null) Instance.FadeToScene(sceneName);
        else SceneManager.LoadScene(sceneName);
    }
}