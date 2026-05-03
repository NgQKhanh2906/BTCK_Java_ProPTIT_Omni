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
    
    [Header("Respawn effect")] 
    [SerializeField] private GameObject respawnVFXPrefab;

    public event Action<int, int> OnLivesChanged;

    private int _lives1;
    private int _lives2;

    private PlayerBase player1;
    private PlayerBase player2;

    private bool isRespawning1 = false;
    private bool isRespawning2 = false;

    public override void Awake()
    {
        base.Awake();
        _lives1 = startingLives;
        _lives2 = startingLives;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnMapLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnMapLoaded;
    }
    private void OnMapLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu") return;
        StartCoroutine(SetupPlayersAfterLoad());
    }

    private IEnumerator SetupPlayersAfterLoad()
    {
        yield return null;
        HandlePlayerConnection();
        CheckLoadDeadState();
    }

    private void HandlePlayerConnection()
    {
        
        if (!player1 )
        {
            GameObject p1Obj = GameObject.FindGameObjectWithTag("Player1");
            if (p1Obj)
            {
                player1 = p1Obj.GetComponent<PlayerBase>();
                if (player1) player1.OnDeath += OnPlayer1Died;
            }
        }
        
        if (!player2 )
        {
            GameObject p2Obj = GameObject.FindGameObjectWithTag("Player2");
            if (p2Obj)
            {
                player2 = p2Obj.GetComponent<PlayerBase>();
                if (player2) player2.OnDeath += OnPlayer2Died;
            }
        }
    }

    private void CheckLoadDeadState()
    {
        if (player1&& player1.CurrentHP <= 0 && !isRespawning1)
        {
            if (_lives1 > 0)
            {
                StartCoroutine(RespawnAfterLoad(1));
            }
            else
            {
                TransformToCorpse(player1);
            }
        }
        if (player2 && player2.CurrentHP <= 0 && !isRespawning2)
        {
            if (_lives2 > 0)
            {
                StartCoroutine(RespawnAfterLoad(2));
            }
            else
            {
                TransformToCorpse(player2);
            }
        }
    }

    private void OnPlayer1Died() => PlayerDied(1);
    private void OnPlayer2Died() => PlayerDied(2);
    
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
    private void TransformToCorpse(PlayerBase p)
    {
        if (!p) return;
        p.tag = "Untagged";
        p.gameObject.layer = LayerMask.NameToLayer("Corpse");
        Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static; 
        }
        Collider2D col = p.GetComponent<Collider2D>();
        if (col)
        {
            col.enabled = false;
        }
        
    }

    public int GetLives(int playerIndex) => playerIndex == 1 ? _lives1 : _lives2;

    private void PlayerDied(int playerIndex)
    {
        PlayerBase deadPlayer = (playerIndex == 1) ? player1 : player2;
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

        bool conMang = (playerIndex == 1) ? _lives1 > 0 : _lives2 > 0;

        if (conMang)
        {
            StartCoroutine(RespawnRoutine(playerIndex, deadPlayer));
        }
        else
        {
            TransformToCorpse(deadPlayer);
        }
    }

    private IEnumerator RespawnRoutine(int playerIndex, PlayerBase player)
    {
        if (playerIndex == 1) isRespawning1 = true;
        else isRespawning2 = true;
        yield return new WaitForSeconds(respawnDelay);
        if (player)
        {
            Vector3 spawnPos = player.LastSafePos;
            if (respawnVFXPrefab)
            {
                GameObject vfx = Instantiate(respawnVFXPrefab, spawnPos, Quaternion.identity);
                Destroy(vfx, 1f);
            }
            player.Respawn(spawnPos);
        }
        if (playerIndex == 1) isRespawning1 = false;
        else isRespawning2 = false;
    }

    private IEnumerator RespawnAfterLoad(int index)
    {
        if (index == 1) isRespawning1 = true;
        else isRespawning2 = true;
        yield return new WaitForSeconds(0.1f);
        PlayerBase p = (index == 1) ? player1 : player2;
        if (p) StartCoroutine(RespawnRoutine(index, p));

        if (index == 1) isRespawning1 = false;
        else isRespawning2 = false;
    }

    private void OnDestroy()
    {
        if (player1 != null) player1.OnDeath -= OnPlayer1Died;
        if (player2 != null) player2.OnDeath -= OnPlayer2Died;
    }
}