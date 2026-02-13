using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
    public string LoadLevelTwo = "Level Two";
    public string LoadLevelThree = "Level Three";

    [Header("Play Movement")]
    public Transform playObject;
    public float playMoveAmount = 2f;
    public float playMoveDuration = 0.5f;
    #endregion
    #region Private
    private bool isPaused = false;

    public static string lastSceneName = "";

    private RectTransform slideRect;
    private bool slideOpen = false;
    private Coroutine slideRoutine;

    private Vector3 returnPos;
    private Quaternion returnRot;
    private Coroutine playMoveRoutine;
    private Coroutine cameraMoveRoutine;
    public float cameraMoveDuration = 0.5f;

    private Animator pauseCameraAnimator;
    #endregion
    #region Start up
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

            pauseCameraAnimator = pauseUICamera.GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (helpPanel != null && helpPanel.activeSelf)
            return;

        if (IsInMainMenu())
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
    #endregion
    #region Camera Controls
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

    void SwitchToLoseCamera()
    {
        DisableAllCameras();
        if (loseCamera != null) loseCamera.enabled = true;
    }

    void SwitchToCreditsUICamera()
    {
        DisableAllCameras();
        if (creditsUICamera != null) creditsUICamera.enabled = true;
    }

    void MoveToCamera(Camera targetCamera)
    {
        if (targetCamera == null)
            return;

        if (cameraMoveRoutine != null)
            StopCoroutine(cameraMoveRoutine);

        cameraMoveRoutine = StartCoroutine(
            MoveCamera(
                targetCamera.transform.position,
                targetCamera.transform.rotation
            )
        );
    }

    IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot)
    {
        float elapsed = 0f;

        Vector3 startPos = pauseUICamera.transform.position;
        Quaternion startRot = pauseUICamera.transform.rotation;

        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / cameraMoveDuration;

            pauseUICamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            pauseUICamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        pauseUICamera.transform.position = targetPos;
        pauseUICamera.transform.rotation = targetRot;
    }
    #endregion
    #region UI Logic
    public void PlayGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (playObject == null)
            return;

        if (playMoveRoutine != null)
            StopCoroutine(playMoveRoutine);

        playMoveRoutine = StartCoroutine(MovePlayObjectUp());
    }

    public void ToggleSlidePanel()
    {
        if (slideRect == null)
            return;

        ExtraButton.transform.localRotation *= Quaternion.Euler(0, 0, 180);
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
        returnPos = pauseUICamera.transform.position;
        returnRot = pauseUICamera.transform.rotation;
        MoveToCamera(helpUICamera);
        if (helpPanel != null) helpPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void CloseHelpPanel()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (cameraMoveRoutine != null)
            StopCoroutine(cameraMoveRoutine);

        cameraMoveRoutine = StartCoroutine(
            MoveCamera(returnPos, returnRot)
        );
    }

    public void OpenCredits()
    {
        returnPos = pauseUICamera.transform.position;
        returnRot = pauseUICamera.transform.rotation;
        MoveToCamera(creditsUICamera);
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void CloseCreditsPanel()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (cameraMoveRoutine != null)
            StopCoroutine(cameraMoveRoutine);

        cameraMoveRoutine = StartCoroutine(
            MoveCamera(returnPos, returnRot)
        );
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

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        HideCursor();

        if (pausePanel != null) pausePanel.SetActive(false);
        if (GameplayUI != null) GameplayUI.SetActive(true);

        SwitchToGameplayCamera();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToPreviousScene()
    {
        if (!string.IsNullOrEmpty(lastSceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(lastSceneName);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void PlayLoseScene()
    {
        isPaused = true;
        Time.timeScale = 0f;

        SwitchToLoseCamera();

        if (GameplayUI != null) GameplayUI.SetActive(false);
        if (losePanel != null) losePanel.SetActive(true);

        ShowCursor();
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;

        if (losePanel != null) losePanel.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
    #region Animation
    IEnumerator MovePlayObjectUp()
    {
        float elapsed = 0f;

        Vector3 startPos = playObject.position;
        Vector3 targetPos = startPos + Vector3.up * playMoveAmount;

        while (elapsed < playMoveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / playMoveDuration;

            playObject.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        playObject.position = targetPos;
        SceneManager.LoadScene(LoadLevelOne);
    }
    #endregion
}