using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("Attack Stats")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask targetLayer;

    private float lastAttackTime;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public bool TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger(GameConfig.ANIM_COL_ATTACK); 
            lastAttackTime = Time.time;
            return true; 
        }
        return false; 
    }

    public void ExecuteAttackHit()
    {
        if (attackPoint == null) return;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, targetLayer);
        
        foreach (Collider2D hitTarget in hitTargets)
        {
            if (hitTarget.TryGetComponent(out IDamageable damageable))
            {
                Vector2 knockbackDir = hitTarget.transform.position - transform.position;
                knockbackDir.y += 0.5f; 
                damageable.TakeDamage(damage, knockbackDir.normalized);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}