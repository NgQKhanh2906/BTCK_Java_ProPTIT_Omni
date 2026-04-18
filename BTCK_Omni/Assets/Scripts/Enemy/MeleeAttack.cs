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
<<<<<<< HEAD
    private Entity parentEntity;
    private readonly int animAttack = Animator.StringToHash("Attack"); 
=======
>>>>>>> f96dfe6c241ef41df446f37cc2cd5091798f2a30

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public bool TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
<<<<<<< HEAD
            anim.SetTrigger(animAttack); 
=======
            anim.SetTrigger(GameConfig.ANIM_COL_ATTACK); 
>>>>>>> f96dfe6c241ef41df446f37cc2cd5091798f2a30
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
<<<<<<< HEAD
                int facing = transform.localScale.x > 0 ? 1 : -1;
                Vector2 hitDir = new Vector2(facing, 0.5f);
                
                targetEntity.TakeDamage(damage, hitDir);
                
                Debug.Log("<color=orange>MeleeAttack: Chém trúng " + hitTarget.name + " mất " + damage + " máu!</color>");
=======
                Vector2 knockbackDir = hitTarget.transform.position - transform.position;
                knockbackDir.y += 0.5f; 
                damageable.TakeDamage(damage, knockbackDir.normalized);
>>>>>>> f96dfe6c241ef41df446f37cc2cd5091798f2a30
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