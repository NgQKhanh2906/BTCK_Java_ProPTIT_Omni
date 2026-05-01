using UnityEngine;

public class MenuController : MonoBehaviour
{
    public void OpenSettings()
    {
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_SETTINGS);
    }

    public void OpenTutorial()
    {
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_TUTORIAL);
    }

    public void PlayGame()
    {
        if (GlobalFader.Instance != null)
        {
            GlobalFader.Instance.ChuyenMap("Map1");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Map1");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Đang thoát game..."); 
        Application.Quit();
    }
    public void ContinueGame()
    {
        GameManager.Instance.TogglePauseGame();
    }

    public void QuitToMenu()
    {
        GameManager.Instance.LoadMenu();
    }
}