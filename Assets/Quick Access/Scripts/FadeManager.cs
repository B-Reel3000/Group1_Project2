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

    [Tooltip("Scenes that should NOT auto fade-in (ex: MainMenu). These scenes will FORCE-CLEAR the fade to transparent on load.")]
    public List<string> autoFadeInExcludeScenes = new List<string> { "MainMenu" };

    Coroutine fadeRoutine;

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
        if (fadeGroup == null) return;

        bool excluded = autoFadeInExcludeScenes != null && autoFadeInExcludeScenes.Contains(scene.name);

        // ✅ CRITICAL FIX:
        // If excluded (MainMenu), always clear the fade so the background scenery is visible.
        if (excluded)
        {
            StopFadeRoutine();
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            return;
        }

        if (!autoFadeInOnSceneLoad) return;

        // Start black then fade in
        StopFadeRoutine();
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;
        fadeRoutine = StartCoroutine(Fade(0f, fadeInTime));
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeToSceneRoutine(sceneName));
    }

    IEnumerator FadeToSceneRoutine(string sceneName)
    {
        Time.timeScale = 1f;

        // Fade out to black
        yield return Fade(1f, fadeOutTime);

        SceneManager.LoadScene(sceneName);

        // Wait 1 frame so scene initializes
        yield return null;

        // If auto fade-in is disabled, handle fade-in here.
        // If enabled, OnSceneLoaded will handle it (or clear it for excluded scenes).
        if (!autoFadeInOnSceneLoad && fadeGroup != null)
            yield return Fade(0f, fadeInTime);
    }

    public void FadeInNow(float duration = -1f)
    {
        if (fadeGroup == null) return;
        if (duration < 0f) duration = fadeInTime;

        StopFadeRoutine();
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;
        fadeRoutine = StartCoroutine(Fade(0f, duration));
    }

    public void FadeOutNow(float duration = -1f)
    {
        if (fadeGroup == null) return;
        if (duration < 0f) duration = fadeOutTime;

        StopFadeRoutine();
        fadeGroup.blocksRaycasts = true;
        fadeRoutine = StartCoroutine(Fade(1f, duration));
    }

    void StopFadeRoutine()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
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

        fadeRoutine = null;
    }

    public static void FadeToSceneSafe(string sceneName)
    {
        if (Instance != null) Instance.FadeToScene(sceneName);
        else SceneManager.LoadScene(sceneName);
    }
}