using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class PlayerLivesUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Image iconLive;
 
    [Header("Player index")]
    [SerializeField] private int playerIndex = 1;
 
    private void Start()
    {
        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.OnLivesChanged += OnLivesChanged;
            UpdateDisplay(LivesManager.Instance.GetLives(playerIndex));
        }
        else
        {
            Debug.LogError("[PlayerLivesUI] Không tìm thấy LivesManager");
        }
    }
 
    private void OnLivesChanged(int index, int newLives)
    {
        if (index != playerIndex) return;
        UpdateDisplay(newLives);
    }
 
    private void UpdateDisplay(int lives)
    {
        if (livesText)
            livesText.text = $"{lives}";
        if (iconLive)
            iconLive.color = lives > 0 ? Color.white : new Color(1f, 1f, 1f, 0.3f);
    }
 
    private void OnDestroy()
    {
        if (LivesManager.Instance != null)
            LivesManager.Instance.OnLivesChanged -= OnLivesChanged;
    }
}