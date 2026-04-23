using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private bool isPaused = false;
    
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name != "Menu")
        {
            TogglePauseGame();
        }
    }

    public void TogglePauseGame()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            PanelManager.Instance.OpenPanel(GameConfig.PANEL_PAUSE);
        }
        else
        {
            Time.timeScale = 1f; 
            PanelManager.Instance.ClosePanel(GameConfig.PANEL_PAUSE);
        }
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadMenu() { LoadScene("Menu"); }
    public void LoadMap1() { LoadScene("Map1"); }
}