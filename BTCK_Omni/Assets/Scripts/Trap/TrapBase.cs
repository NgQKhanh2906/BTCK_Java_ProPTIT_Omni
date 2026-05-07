using UnityEngine;

public class TrapBase : MonoBehaviour
{
    [SerializeField] protected float damage = 20f;
    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        DealDamage(col.gameObject);
    }
    private void DealDamage(GameObject hitObj)
    {
        IDamageable d = hitObj.GetComponentInParent<IDamageable>();
        
        if (d != null)
        {
            if (d.IsDead()) return;
            Vector2 dir = (hitObj.transform.position - transform.position).normalized;
            d.TakeDamage(damage, dir);
        }
    }
}