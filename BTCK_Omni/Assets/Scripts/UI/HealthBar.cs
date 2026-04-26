using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    
    private Entity _entity;
 
    private void Awake()
    {
        if (!slider)
            slider = GetComponent<Slider>();
    }
    
    public void Init(Entity entity)
    {
        if (entity == null) return;
        if (_entity != null)
            _entity.OnHPChanged -= UpdateBar;
        _entity = entity;
        slider.minValue = 0f;
        slider.maxValue = entity.MaxHP;
        slider.value = entity.CurrentHP;
        _entity.OnHPChanged += UpdateBar;
    }
    
    private void UpdateBar(float current, float max)
    {
        slider.maxValue = max;
        slider.DOValue(current, 0.3f).SetEase(Ease.OutQuad);
    }
    
    private void OnDestroy()
    {
        slider.DOKill();
        if (_entity != null)
            _entity.OnHPChanged -= UpdateBar;
    }
}