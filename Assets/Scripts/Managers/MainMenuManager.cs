using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game Scene");
        
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}