using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    
    [SerializeField] private PlayerBase player1;
    [SerializeField] private PlayerBase player2;
    
    public void SetScenePlayers(PlayerBase p1, PlayerBase p2)
    {
        player1 = p1;
        player2 = p2;
    }
    
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
        PlayerDataManager.Instance?.SaveBeforeSceneChange(player1, player2);
        Time.timeScale = 1f;
        isPaused = false;
        if (GlobalFader.Instance != null) GlobalFader.Instance.ChuyenMap(sceneName);
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
    public void NewGame()
    {
        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.Clear();
        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.SetLivesDirectly(1, 2); 
            LivesManager.Instance.SetLivesDirectly(2, 2);
        }
        LoadScene("Map1");
    }
 
    public void ContinueFromSave()
    {
        SaveSystem.Instance?.Load();
    }
}