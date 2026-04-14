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
    private Entity parentEntity;
    private readonly int animAttack = Animator.StringToHash("Attack"); 

    private void Awake()
    {
        anim = GetComponent<Animator>();
        parentEntity = GetComponent<Entity>();
    }

    public bool TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger(animAttack); 
            lastAttackTime = Time.time;
            return true; 
        }
        return false; 
    }

    public void ExecuteAttackHit()
    {
        if (attackPoint == null) return;
        Collider2D hitTarget = Physics2D.OverlapCircle(attackPoint.position, attackRadius, targetLayer);
        
        if (hitTarget != null)
        {
            if (hitTarget.TryGetComponent(out Entity targetEntity))
            {
                int facing = transform.localScale.x > 0 ? 1 : -1;
                Vector2 hitDir = new Vector2(facing, 0.5f);
                
                targetEntity.TakeDamage(damage, hitDir);
                
                Debug.Log("<color=orange>MeleeAttack: Chém trúng " + hitTarget.name + " mất " + damage + " máu!</color>");
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