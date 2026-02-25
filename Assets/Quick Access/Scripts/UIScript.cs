using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using System.Collections;

public class UIScript : MonoBehaviour
{
    #region Public
    [Header("Cameras")]
    public Camera gameplayCamera;
    public Camera cineCamera;
    public Camera pauseUICamera;
    public Camera helpUICamera;
    public Camera creditsUICamera;
    public Camera loseCamera;

    [Header("Extra Panel")]
    public GameObject slidePanel;
    public Button ExtraButton;
    public float slideDuration = 0.4f;
    public Vector2 hiddenPosition;
    public Vector2 shownPosition;

    [Header("UI Panels")]
    public GameObject helpPanel;
    public GameObject pausePanel;
    public GameObject creditsPanel;
    public GameObject losePanel;

    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button helpButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Help Menu Buttons")]
    public Button helpBackButton;

    [Header("Credits Buttons")]
    public Button CreditsBackButton;

    [Header("Lose Buttons")]
    public Button retryButton;
    public Button loseHelpButton;
    public Button menuReturnButton;

    [Header("Hide UI")]
    public GameObject GameplayUI;

    [Header("Scene Loading")]
    public string LoadLevelOne = "Level One";

    [Header("Main Menu Timeline")]
    public PlayableDirector playDirector;
    #endregion

    #region Private
    private bool isPaused = false;
    private bool playSequenceStarted = false;

    private RectTransform slideRect;
    private bool slideOpen = false;
    private Coroutine slideRoutine;

    private Vector3 returnPos;
    private Quaternion returnRot;
    private Coroutine cameraMoveRoutine;
    public float cameraMoveDuration = 0.5f;
    #endregion

    bool IsInMainMenu()
    {
        return SceneManager.GetActiveScene().name == "MainMenu";
    }

    void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
    }

    void Start()
    {
        SwitchToGameplayCamera();

        if (slidePanel != null)
        {
            slideRect = slidePanel.GetComponent<RectTransform>();
            slideRect.anchoredPosition = hiddenPosition;
        }

        // Button hookups
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (helpButton != null) helpButton.onClick.AddListener(OpenHelpPanel);
        if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (helpBackButton != null) helpBackButton.onClick.AddListener(CloseHelpPanel);
        if (CreditsBackButton != null) CreditsBackButton.onClick.AddListener(CloseCreditsPanel);
        if (retryButton != null) retryButton.onClick.AddListener(RetryLevel);
        if (loseHelpButton != null) loseHelpButton.onClick.AddListener(OpenHelpPanel);
        if (menuReturnButton != null) menuReturnButton.onClick.AddListener(ReturnToMainMenu);
        if (ExtraButton != null) ExtraButton.onClick.AddListener(ToggleSlidePanel);

        if (helpPanel != null) helpPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        if (IsInMainMenu())
        {
            SwitchToPauseCamera();
            ShowCursor();

            if (pausePanel != null) pausePanel.SetActive(true);

            // THE IMPORTANT FIX
            // Completely disable timeline so it cannot evaluate at startup
            if (playDirector != null)
            {
                playDirector.Stop();
                playDirector.time = 0;
                playDirector.enabled = false;
                playSequenceStarted = false;
            }
        }
    }

    void Update()
    {
        if (helpPanel != null && helpPanel.activeSelf) return;
        if (IsInMainMenu()) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    #region Camera
    void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DisableAllCameras()
    {
        if (gameplayCamera != null) gameplayCamera.enabled = false;
        if (cineCamera != null) cineCamera.enabled = false;
        if (pauseUICamera != null) pauseUICamera.enabled = false;
        if (helpUICamera != null) helpUICamera.enabled = false;
        if (loseCamera != null) loseCamera.enabled = false;
        if (creditsUICamera != null) creditsUICamera.enabled = false;
    }

    void SwitchToGameplayCamera()
    {
        DisableAllCameras();
        if (gameplayCamera != null) gameplayCamera.enabled = true;
    }

    void SwitchToPauseCamera()
    {
        DisableAllCameras();
        if (pauseUICamera != null) pauseUICamera.enabled = true;
    }
    #endregion

    #region Menu Actions

    public void PlayGame()
    {
        if (playSequenceStarted) return;
        playSequenceStarted = true;

        if (pausePanel != null) pausePanel.SetActive(false);

        if (playDirector != null)
        {
            // Enable timeline ONLY now
            playDirector.enabled = true;
            playDirector.Stop();
            playDirector.time = 0;
            playDirector.Evaluate();
            playDirector.Play();
        }
    }

    // CALLED BY TIMELINE SIGNAL AT END
    public void LoadLevelOneScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(LoadLevelOne);
    }

    public void ToggleSlidePanel()
    {
        if (slideRect == null) return;

        slideOpen = !slideOpen;

        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(
            SlidePanel(slideOpen ? shownPosition : hiddenPosition)
        );
    }

    IEnumerator SlidePanel(Vector2 targetPos)
    {
        Vector2 startPos = slideRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;
            slideRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        slideRect.anchoredPosition = targetPos;
    }

    public void OpenHelpPanel()
    {
        if (helpPanel != null) helpPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void CloseHelpPanel()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void OpenCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void CloseCreditsPanel()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        HideCursor();

        if (pausePanel != null) pausePanel.SetActive(false);
        if (GameplayUI != null) GameplayUI.SetActive(true);

        SwitchToGameplayCamera();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            ShowCursor();

            if (pausePanel != null) pausePanel.SetActive(true);
            if (GameplayUI != null) GameplayUI.SetActive(false);

            SwitchToPauseCamera();
        }
        else
        {
            Time.timeScale = 1f;
            HideCursor();

            if (pausePanel != null) pausePanel.SetActive(false);
            if (GameplayUI != null) GameplayUI.SetActive(true);

            SwitchToGameplayCamera();
        }
    }
    #endregion
}