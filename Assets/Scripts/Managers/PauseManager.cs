using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject firstPauseButton;
    [SerializeField] private GameObject firstAudioSlider;
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private InputManager inputManager;

    private bool _isPaused;

    private void OnEnable()
    {
        inputManager.Pause += TogglePause;
    }

    private void OnDisable()
    {
        inputManager.Pause -= TogglePause;
    }

    public void TogglePause()
    {
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

    public void GoToMain()
    {
        
        SceneManager.LoadScene("Main Menu");
    }
}


