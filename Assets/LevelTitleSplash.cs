using UnityEngine;
using System.Collections;

public class LevelTitleSplash : MonoBehaviour
{
    [Header("Show / Hide Timing")]
    public float showDuration = 1.25f;
    public float fadeOutDuration = 0.35f;

    [Header("Optional Fade")]
    public CanvasGroup canvasGroup; // optional; if null, it will just disable at end

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hipcrackClip;
    public float volume = 1f;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // reset
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        // play sound
        if (audioSource != null && hipcrackClip != null)
            audioSource.PlayOneShot(hipcrackClip, volume);

        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        // stay visible
        yield return new WaitForSeconds(showDuration);

        // fade out if possible
        if (canvasGroup != null && fadeOutDuration > 0.01f)
        {
            float t = 0f;
            float start = canvasGroup.alpha;

            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                float p = t / fadeOutDuration;
                canvasGroup.alpha = Mathf.Lerp(start, 0f, p);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        // hide
        gameObject.SetActive(false);
    }
}