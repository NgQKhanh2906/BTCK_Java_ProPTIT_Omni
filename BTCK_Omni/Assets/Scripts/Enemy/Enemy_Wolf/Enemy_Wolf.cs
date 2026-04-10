using UnityEngine;

public class Enemy_Wolf : EnemyBase
{
    [Header("Detection & Attack Range")]
    [SerializeField] private bool playerInDetectionRange;
    [SerializeField] private bool playerInAttackRange;
    private Transform playerTransform;
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCheckRadius = 1.5f; 
    [SerializeField] private Transform attackCheckPoint; 
    [SerializeField] private LayerMask whatIsPlayer; 
    [SerializeField] private float attackCooldown = 1.5f; 
    private float lastAttackTime;
    [Header("Wolf Knockback Settings")]
    [SerializeField] private float wolfKnockbackDuration = 0.2f;
    private float wolfKnockbackTimer;
    protected override void Awake()
    {
        base.Awake(); 
    }

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        base.TakeDamage(dmg, hitDir);
        if (!isDead)
        {
            wolfKnockbackTimer = wolfKnockbackDuration;
        }
    }

    protected override void Update()
    {
        if (isDead) return;

        if (wolfKnockbackTimer > 0)
        {
            wolfKnockbackTimer -= Time.deltaTime;
            return; 
        }
        if (playerInAttackRange)
        {
            AttackLogic();
        }
        else if (playerInDetectionRange && playerTransform != null)
        {
            ChaseLogic();
        }
        else
        {
            base.Update();
        }
    }
    private void ChaseLogic()
    {
        isAdle = false;
        UpdateAnimation(true);
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        int moveDir = directionToPlayer > 0 ? 1 : -1;
        if (moveDir != facingDir)
        {
            Flip();
        }
        SetVelocityX(moveSpeed * 1.5f * facingDir);
    }

    private void AttackLogic()
    {
        if (isDead) return; 
        SetVelocityX(0); 
        UpdateAnimation(false);
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (!isDead && !anim.GetCurrentAnimatorStateInfo(0).IsName("Enemy_Wolf_Attack"))
            {
                anim.SetTrigger("Attack");
                lastAttackTime = Time.time; 
            }
        }
    }
    public void AnimationAttackTrigger()
    {
        if (attackCheckPoint == null) return;
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackCheckPoint.position, attackCheckRadius, whatIsPlayer);

        if (hitPlayer != null)
        {
            if (hitPlayer.TryGetComponent(out Entity targetEntity))
            {
                Vector2 hitDir = new Vector2(facingDir, 0.5f);

                targetEntity.TakeDamage(attackDamage, hitDir);
                Debug.Log("Sói đã cắn trúng Player và gây " + attackDamage + " dame!");
            }
        }
    }

    public void SetPlayerDetection(bool inRange, Transform player)
    {
        playerInDetectionRange = inRange;
        playerTransform = inRange ? player : null;
    }
    public void SetPlayerAttack(bool inRange)
    {
        playerInAttackRange = inRange;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackCheckPoint.position, attackCheckRadius);
        }
    }
}