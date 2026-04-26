using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private ManaBar manaBar;
    [SerializeField] private Image avatarImage;
    [SerializeField] private PlayerBase player;

    private void Start()
    {
        if (player == null)
        {
            return;
        }

        healthBar?.Init(player);
        manaBar?.Init(player);
        player.OnDeath += OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        if (avatarImage)
            avatarImage.color = new Color(1f, 1f, 1f, 0.4f);
    }

    private void OnDestroy()
    {
        if (player != null)
            player.OnDeath -= OnPlayerDied;
    }
}