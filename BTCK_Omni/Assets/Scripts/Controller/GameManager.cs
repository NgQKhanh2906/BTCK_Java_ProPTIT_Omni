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
    
    public void LoadMenu() 
    { 
        StartCoroutine(LoadMenuSequence()); 
    }

    private System.Collections.IEnumerator LoadMenuSequence()
    {
        Time.timeScale = 1f;
        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.ToiDan());
        }
        
        PanelManager.Instance.CloseAllPanels();
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.SangDan());
        }
    }
    public void LoadMap1() { LoadScene("Map1"); }
    public void Victory()
    {
        Debug.Log("CHIẾN THẮNG! Đang chạy hiệu ứng chuyển cảnh sang WinPanel...");
        StartCoroutine(VictorySequence());
    }

    private System.Collections.IEnumerator VictorySequence()
    {
        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.ToiDan());
        }
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_WIN);
        Time.timeScale = 0f;
        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.SangDan());
        }
    }
    
    private void CheckGameOver()
    {
        // if (p1_lives == 0 && p2_lives == 0)
        // {
        //     Debug.Log("Cả 2 đã hết mạng. GAME OVER!");
        //     
        //     PanelManager.Instance.OpenPanel(GameConfig.PANEL_LOSE);
        //     Time.timeScale = 0f; 
        // }
    }
}