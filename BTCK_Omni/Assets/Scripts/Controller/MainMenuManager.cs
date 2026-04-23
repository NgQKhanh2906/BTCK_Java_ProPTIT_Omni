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
        GameManager.Instance.LoadMap1();
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
        PanelManager.Instance.CloseAllPanels(); 
        GameManager.Instance.LoadMenu();
    }
}