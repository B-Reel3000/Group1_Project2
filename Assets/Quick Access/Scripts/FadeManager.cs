using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("UI")]
    public CanvasGroup fadeGroup;     // drag the Panel's CanvasGroup here

    [Header("Timing")]
    public float fadeOutTime = 0.6f;
    public float fadeInTime = 0.6f;

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
    }

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

        yield return Fade(0f, fadeInTime);
    }

    IEnumerator Fade(float target, float duration)
    {
        if (fadeGroup == null) yield break;

        fadeGroup.blocksRaycasts = true;

        float start = fadeGroup.alpha;
        float t = 0f;

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
}