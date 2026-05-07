using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
 
public class ManaBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
 
    private PlayerBase _player;
 
    private void Awake()
    {
        if (!slider)
            slider = GetComponent<Slider>();
    }
 
    public void Init(PlayerBase player)
    {
        if (!player) return;
 
        if (_player != null)
            _player.OnManaChanged -= UpdateBar;
 
        _player = player;
 
        slider.minValue = 0f;
        slider.maxValue = player.MaxMana;
        slider.value = player.CurrentMana;
 
        _player.OnManaChanged += UpdateBar;
    }
 
    private void UpdateBar(float current, float max)
    {
        slider.maxValue = max;
        slider.DOValue(current,0.3f).SetEase(Ease.OutQuad);
    }
 
    private void OnDestroy()
    {
        slider.DOKill();
        if (_player != null)
            _player.OnManaChanged -= UpdateBar;
    }
}