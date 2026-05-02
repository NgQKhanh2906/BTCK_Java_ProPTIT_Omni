using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LivesManager : Singleton<LivesManager>
{
    [Header("Cài đặt mạng")] 
    [SerializeField] private int startingLives = 2;
    [SerializeField] private int maxLives = 9;
    [SerializeField] private float respawnDelay = 2f;
    [Header("References")] 
    [SerializeField] private PlayerBase player1;
    [SerializeField] private PlayerBase player2;
    [Header("Respawn effect")] 
    [SerializeField] private GameObject respawnVFXPrefab;

    public event Action<int, int> OnLivesChanged;

    private int _lives1;
    private int _lives2;

    public override void Awake()
    {
        base.Awake();
        _lives1 = startingLives;
        _lives2 = startingLives;
    }

    private void Start()
    {
        if (player1 != null) player1.OnDeath += () => PlayerDied(1);
        if (player2 != null) player2.OnDeath += () => PlayerDied(2);
    }

    public void SetPlayers(PlayerBase p1, PlayerBase p2)
    {
        if (player1 != null) player1.OnDeath -= () => PlayerDied(1);
        if (player2 != null) player2.OnDeath -= () => PlayerDied(2);
        player1 = p1;
        player2 = p2;
        if (player1 != null) player1.OnDeath += () => PlayerDied(1);
        if (player2 != null) player2.OnDeath += () => PlayerDied(2);
    }

    public void SetLivesDirectly(int index, int lives)
    {
        if (index == 1)
        {
            _lives1 = Mathf.Clamp(lives, 0, maxLives);
            OnLivesChanged?.Invoke(1, _lives1);
        }
        else
        {
            _lives2 = Mathf.Clamp(lives, 0, maxLives);
            OnLivesChanged?.Invoke(2, _lives2);
        }
    }

    public int GetLives(int playerIndex) =>
        playerIndex == 1 ? _lives1 : _lives2;

    public void AddLife(int playerIndex)
    {
        if (playerIndex == 1)
        {
            _lives1 = Mathf.Min(_lives1 + 1, maxLives);
            OnLivesChanged?.Invoke(1, _lives1);
        }
        else
        {
            _lives2 = Mathf.Min(_lives2 + 1, maxLives);
            OnLivesChanged?.Invoke(2, _lives2);
        }
    }

    private void PlayerDied(int playerIndex)
    {
        PlayerBase deadPlayer = playerIndex == 1 ? player1 : player2;
        if (!deadPlayer) return;
        if (playerIndex == 1)
        {
            _lives1 = Mathf.Max(0, _lives1 - 1);
            OnLivesChanged?.Invoke(1, _lives1);
        }
        else
        {
            _lives2 = Mathf.Max(0, _lives2 - 1);
            OnLivesChanged?.Invoke(2, _lives2);
        }
        bool hasLives = playerIndex == 1 ? _lives1 > 0 : _lives2 > 0;
        if (hasLives)
        {
            StartCoroutine(RespawnRoutine(playerIndex));
        }
        else
        {
            CheckGameOver();
        }
    }

    private IEnumerator RespawnRoutine(int playerIndex)
    {
        yield return new WaitForSeconds(respawnDelay);
        PlayerBase player = playerIndex == 1 ? player1 : player2;
        Vector3 deathPos = player.LastSafePos;
        if (respawnVFXPrefab != null)
        {
            GameObject vfx = Instantiate(respawnVFXPrefab, deathPos, Quaternion.identity);
            Destroy(vfx, respawnDelay);
        }
        if (!player) yield break;
        player.Respawn(deathPos);
    }

    private void CheckGameOver()
    {
        bool p1Dead = !player1 || _lives1 <= 0;
        bool p2Dead = !player2 || _lives2 <= 0;
        if (p1Dead && p2Dead)
        {
            StartCoroutine(GameOverRoutine());
        }
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        _lives1 = startingLives;
        _lives2 = startingLives;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (player1 != null) player1.OnDeath -= () => PlayerDied(1);
        if (player2 != null) player2.OnDeath -= () => PlayerDied(2);
    }
}