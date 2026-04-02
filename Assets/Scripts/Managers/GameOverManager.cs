using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DeathManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject firstGameOverButton;
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private AudioSource gameoverSFXSource;
    
    
    private bool _isGameOver;

    private void Start()
    {
        playerStatus.OnDeath += HandlePlayerDeath;
    }

    private void OnDestroy()
    {
        if (playerStatus != null)
            playerStatus.OnDeath -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        ShowGameOver();
    }

    public void ShowGameOver()
    {
        _isGameOver = true;
        
        Time.timeScale = 0f;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        playerHUD.SetActive(false);
        playerModel.SetActive(false);
        gameOverPanel.SetActive(true);

        gameoverSFXSource.Play();
        
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


    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }
}
