using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource gameOverSFXSource;

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private GameObject firstPauseButton;
    [SerializeField] private GameObject firstAudioSlider;
    [SerializeField] private GameObject firstGameOverButton;

    [Header("Game")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private PlayerStatus playerStatus;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.3f;

    private bool _isPaused;
    private bool _isGameOver;

    private void Start()
    {   
        inputManager.Pause += TogglePause;
        playerStatus.OnDeath += HandlePlayerDeath;

        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Play();
        }
    }

    private void OnDestroy()
    {
        if (inputManager != null)
            inputManager.Pause -= TogglePause;
        if (playerStatus != null)
            playerStatus.OnDeath -= HandlePlayerDeath;
    }

    #region PAUSE SYSTEM

    public void TogglePause()
    {
        if (_isGameOver)
            return;

        if (_isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        _isPaused = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerHUD.SetActive(false);
        pausePanel.SetActive(true);

        inputManager.EnableUI();
    }

    public void Resume()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pausePanel.SetActive(false);
        audioPanel.SetActive(false);
        playerHUD.SetActive(true);

        inputManager.EnableGameplay();
    }

    public void ChangeToAudioSettings()
    {
        audioPanel.SetActive(true);
        pausePanel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstAudioSlider);
    }

    public void ChangeToPauseMenu()
    {
        audioPanel.SetActive(false);
        pausePanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstPauseButton);
    }

    #endregion

    #region DEATH SYSTEM

    private void HandlePlayerDeath()
    {
        playerModel.SetActive(false);
        ShowGameOver();
    }

    public void ShowGameOver()
    {
        _isGameOver = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerHUD.SetActive(false);
        gameOverPanel.SetActive(true);

        inputManager.EnableUI();
        
        if (gameOverSFXSource != null)
        {
            gameOverSFXSource.Play();
            backgroundMusicSource.Stop();
        }

        StartCoroutine(ExpandGameOverRoutine());
    }

    private IEnumerator ExpandGameOverRoutine()
    {
        RectTransform rectTransform = gameOverPanel.GetComponent<RectTransform>();
        
        rectTransform.localScale = new Vector3(1.5f, 0f, 1f);
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeInDuration);
            
            rectTransform.localScale = new Vector3(1.5f, progress, 1f);
            yield return null;
        }
        
        rectTransform.localScale = new Vector3(1.5f, 1f, 1f);
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstGameOverButton);
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }

    #endregion

    #region SCENE MANAGEMENT

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    #endregion
}
