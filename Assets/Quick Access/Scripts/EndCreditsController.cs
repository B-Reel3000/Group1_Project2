using UnityEngine;
using TMPro;
using System.Collections;

public class EndCreditsController : MonoBehaviour
{
    [Header("Title (fades out)")]
    public CanvasGroup titleGroup;          // CanvasGroup on the GAME TITLE object
    public float titleHoldTime = 1.5f;      // time title stays visible before fading
    public float titleFadeOutTime = 1.0f;   // fade duration

    [Header("Credits (scroll up)")]
    public RectTransform creditsRoot;       // the parent RectTransform that moves upward (names, etc.)
    public float scrollSpeed = 80f;         // UI units per second
    public float endBuffer = 200f;          // extra distance past top before ending

    [Tooltip("If assigned, used to calculate when credits are fully off-screen.")]
    public RectTransform creditsContent;    // optional: the content inside creditsRoot

    [Header("Scene")]
    public string mainMenuScene = "MainMenu";

    [Header("Finish")]
    public float finishHoldTime = 0.75f;    // small pause at end before fading

    bool running;

    void Start()
    {
        // Make sure time is normal so UI animates
        Time.timeScale = 1f;

        if (!running)
            StartCoroutine(RunCredits());
    }

    IEnumerator RunCredits()
    {
        running = true;

        // --- Title hold ---
        if (titleGroup != null)
        {
            titleGroup.alpha = 1f;
            titleGroup.blocksRaycasts = false;
            titleGroup.interactable = false;
        }

        yield return new WaitForSecondsRealtime(titleHoldTime);

        // --- Title fade out ---
        if (titleGroup != null && titleFadeOutTime > 0.01f)
        {
            float t = 0f;
            float start = titleGroup.alpha;

            while (t < titleFadeOutTime)
            {
                t += Time.unscaledDeltaTime;
                float p = t / titleFadeOutTime;
                titleGroup.alpha = Mathf.Lerp(start, 0f, p);
                yield return null;
            }

            titleGroup.alpha = 0f;
        }

        // --- Scroll credits until fully off-screen ---
        if (creditsRoot != null)
        {
            // Determine end position using Canvas height
            Canvas canvas = creditsRoot.GetComponentInParent<Canvas>();
            RectTransform canvasRT = canvas != null ? canvas.GetComponent<RectTransform>() : null;

            float canvasHeight = canvasRT != null ? canvasRT.rect.height : 1080f;

            // If creditsContent is assigned, use its height, else use creditsRoot height
            float contentHeight = creditsContent != null ? creditsContent.rect.height : creditsRoot.rect.height;

            // We consider "done" when the bottom of the content is above the top of the screen + buffer
            float targetY = canvasHeight + contentHeight + endBuffer;

            while (creditsRoot.anchoredPosition.y < targetY)
            {
                Vector2 pos = creditsRoot.anchoredPosition;
                pos.y += scrollSpeed * Time.unscaledDeltaTime;
                creditsRoot.anchoredPosition = pos;

                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(finishHoldTime);

        // --- Fade to main menu ---
        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene(mainMenuScene);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
    }
}