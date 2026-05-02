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
        if(GameManager.Instance != null) GameManager.Instance.NewGame();
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

    public void ContinueFromSave()
    {
        if(GameManager.Instance != null) GameManager.Instance.ContinueFromSave();
    }
    
    public void QuitToMenu()
    {
        GameManager.Instance.LoadMenu();
    }
}