using UnityEngine;
using TMPro;
using System.Collections;

public class BossIntroUI : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup panelGroup;
    public TMP_Text bossNameText;

    [Header("Timing")]
    public float holdTime = 1.5f;
    public float fadeTime = 0.8f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip whipCrack;

    bool showing;

    void Awake()
    {
        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.blocksRaycasts = false;
        }
    }

    public void ShowBossName(string bossName)
    {
        if (showing) return;

        showing = true;

        if (bossNameText != null)
            bossNameText.text = bossName;

        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        if (panelGroup != null)
            panelGroup.alpha = 1f;

        if (audioSource != null && whipCrack != null)
            audioSource.PlayOneShot(whipCrack);

        yield return new WaitForSeconds(holdTime);

        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;

            if (panelGroup != null)
                panelGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

            yield return null;
        }

        if (panelGroup != null)
            panelGroup.alpha = 0f;

        showing = false;
    }
}