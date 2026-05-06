using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    [Header("Win SFX Settings")]
    [SerializeField]
    private AudioSource winAudioSource;

    [SerializeField] private AudioClip winMusic;

    [SerializeField] private PlayerBase player1;
    [SerializeField] private PlayerBase player2;

    private bool isPaused = false;
    private bool isTransitioning = false;
    private bool isGameOver = false;

    public void SetScenePlayers(PlayerBase p1, PlayerBase p2)
    {
        player1 = p1;
        player2 = p2;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu") return;

        GameObject p1Obj = GameObject.FindGameObjectWithTag("Player1");
        GameObject p2Obj = GameObject.FindGameObjectWithTag("Player2");

        if (p1Obj != null) player1 = p1Obj.GetComponent<PlayerBase>();
        if (p2Obj != null) player2 = p2Obj.GetComponent<PlayerBase>();
    }

    private void Update()
    {
        if (isTransitioning)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name != "Menu")
        {
            TogglePauseGame();
        }

        if (SceneManager.GetActiveScene().name != "Menu")
        {
            CheckGameOver();
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
        StartCoroutine(LoadSceneSequence(sceneName));
    }

    private IEnumerator LoadSceneSequence(string sceneName)
    {
        isTransitioning = true;
        PlayerDataManager.Instance?.SaveBeforeSceneChange(player1, player2);

        Time.timeScale = 1f;
        isPaused = false;

        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.ToiDan());
        }

        if (PanelManager.Instance != null) PanelManager.Instance.CloseAllPanels();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.2f);

        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.SangDan());
        }

        isTransitioning = false;
    }

    public void LoadMenu()
    {
        this.enabled = true;
        StartCoroutine(LoadMenuSequence());
    }

    private IEnumerator LoadMenuSequence()
    {
        isTransitioning = true;
        Time.timeScale = 1f;
        isPaused = false;

        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.ToiDan());
        }

        if (PanelManager.Instance != null) PanelManager.Instance.CloseAllPanels();

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

        isTransitioning = false;
    }

    public void Victory()
    {
        Debug.Log("CHIẾN THẮNG! Đang chạy hiệu ứng chuyển cảnh sang WinPanel...");
        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {
        isTransitioning = true;
        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.ToiDan());
        }

        PanelManager.Instance.OpenPanel(GameConfig.PANEL_WIN);
        if (winAudioSource != null && winMusic != null)
        {
            winAudioSource.clip = winMusic;
            winAudioSource.loop = true;
            winAudioSource.Play();
        }

        Time.timeScale = 0f;

        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.SangDan());
        }

        isTransitioning = false;
    }

    private void CheckGameOver()
    {
        if (player1 == null && player2 == null)
        {
            return;
        }

        bool p1Dead = player1 == null || player1.IsDead();
        bool p2Dead = player2 == null || player2.IsDead();
        int lives1 = LivesManager.Instance != null ? LivesManager.Instance.GetLives(1) : 0;
        int lives2 = LivesManager.Instance != null ? LivesManager.Instance.GetLives(2) : 0;
        bool p1Out = p1Dead && (lives1 <= 0);
        bool p2Out = p2Dead && (lives2 <= 0);

        if (p1Out && p2Out && !isGameOver)
        {
            isGameOver = true;
            StartCoroutine(GameOverSequence());
        }
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Cả 2 đã tử trận hoặc hết mạng.");
        PanelManager.Instance.OpenPanel(GameConfig.PANEL_LOSE);
        Time.timeScale = 0f;
        this.enabled = false;
    }

    public void NewGame()
    {
        this.enabled = true;
        StartCoroutine(NewGameSequence());
    }

    private IEnumerator NewGameSequence()
    {
        isGameOver = false;
        isTransitioning = true;
        Time.timeScale = 1f;
        isPaused = false;

        if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.Clear();

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.SetLivesDirectly(1, 2);
            LivesManager.Instance.SetLivesDirectly(2, 2);
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.DeleteSave();
            Debug.Log("Đã xóa file save cũ để bắt đầu New Game!");
        }

        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.ToiDan());
        }

        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.CloseAllPanels();
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Cutscene_1");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.SangDan());
        }

        Time.timeScale = 1f;
        isTransitioning = false;
    }

    public void ContinueFromSave()
    {
        this.enabled = true;
        StartCoroutine(ContinueSequence());
    }

    private IEnumerator ContinueSequence()
    {
        isGameOver = false;
        isTransitioning = true;
        Time.timeScale = 1f;
        isPaused = false;

        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.ToiDan());
        }

        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.CloseAllPanels();
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.Load();
        }

        yield return new WaitForSecondsRealtime(0.2f);

        if (GlobalFader.Instance != null)
        {
            yield return StartCoroutine(GlobalFader.Instance.SangDan());
        }

        isTransitioning = false;
    }
}