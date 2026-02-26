using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector director;          // drag your PlayableDirector here

    [Header("Next Scene")]
    public string nextScene = "Level Two";     // set to your Level 2 scene name

    [Header("Optional delay after timeline ends")]
    public float endDelay = 0.1f;

    bool alreadyEnding = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (director == null)
            director = FindFirstObjectByType<PlayableDirector>();

        if (director != null)
        {
            director.stopped += OnTimelineStopped;
            director.Play();
        }
        else
        {
            // If no timeline found, just go next
            GoNext();
        }
    }

    void OnDestroy()
    {
        if (director != null)
            director.stopped -= OnTimelineStopped;
    }

    void OnTimelineStopped(PlayableDirector d)
    {
        GoNext();
    }

    void GoNext()
    {
        if (alreadyEnding) return;
        alreadyEnding = true;
        StartCoroutine(GoNextRoutine());
    }

    IEnumerator GoNextRoutine()
    {
        yield return new WaitForSeconds(endDelay);

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene(nextScene);
        else
            SceneManager.LoadScene(nextScene);
    }
}