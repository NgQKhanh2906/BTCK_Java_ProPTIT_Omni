using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    
    public int targetPlayerIndex = 1;

    [SerializeField] private HealthBar healthBar;
    [SerializeField] private ManaBar manaBar;
    [SerializeField] private Image avatarImage;
    
    
    private PlayerBase player; 
    private bool isInit = false;

    private void Update()
    {
        
        if (!isInit) ConnectToPlayer();
    }

    private void ConnectToPlayer()
    {
        string tagToFind = targetPlayerIndex == 1 ? "Player1" : "Player2";
        GameObject pObj = GameObject.FindGameObjectWithTag(tagToFind);
        
        if (pObj)
        {
            player = pObj.GetComponent<PlayerBase>();
            if (player)
            {
                if (healthBar) healthBar.Init(player);
                if (manaBar) manaBar.Init(player);
                player.OnDeath += OnPlayerDied;
                player.OnDeath += OnPlayerDied;
                player.OnRespawnEvent += OnPlayerRespawned;
                PlayerBase p1 = player.playerIndex == 1 ? player : null;
                PlayerBase p2 = player.playerIndex == 2 ? player : null;
                if (GameManager.Instance) GameManager.Instance.SetScenePlayers(p1, p2);
                if (PlayerDataManager.Instance) PlayerDataManager.Instance.RestorePlayerData(p1, p2);
                if (SaveSystem.Instance) SaveSystem.Instance.RestoreAfterLoad(p1, p2);
                isInit = true; 
            }
        }
    }

    private void OnPlayerDied()
    {
        if (avatarImage)
            avatarImage.color = new Color(1f, 1f, 1f, 0.4f);
    }
    private void OnPlayerRespawned()
    {
        if (avatarImage)
            avatarImage.color = Color.white;
    }
    private void OnDestroy()
    {
        if (player != null)
            player.OnDeath -= OnPlayerDied;
    }
}