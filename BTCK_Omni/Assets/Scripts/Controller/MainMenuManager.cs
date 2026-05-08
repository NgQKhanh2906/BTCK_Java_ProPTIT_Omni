using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject btnContinue;

    private void Start()
    {
        if (btnContinue != null && SaveSystem.Instance != null)
        {
            btnContinue.SetActive(SaveSystem.Instance.HasSave());
        }
    }

    public void OpenSettings()
    {
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_SETTING);
    }

    public void OpenTutorial()
    {
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_TUTORIAL);
    }

    public void PlayGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.NewGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ContinueGame()
    {
        GameManager.Instance.TogglePauseGame();
    }

    public void ContinueFromSave()
    {
        if (GameManager.Instance != null) GameManager.Instance.ContinueFromSave();
    }

    public void QuitToMenu()
    {
        GameManager.Instance.LoadMenu();
    }

    public void SaveAndQuitToMenu()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.Save();
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMenu();
        }
    }
}