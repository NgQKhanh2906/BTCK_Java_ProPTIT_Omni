using UnityEngine;

public class Enemy_Mushroom : EnemyBase
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

    [Header("Stun Settings (Choáng sau khi đánh)")]
    [SerializeField] private float stunDuration = 2f; 
    private float stunTimer;

    [Header("Mushroom Knockback Settings")]
    [SerializeField] private float mushroomKnockbackDuration = 0.2f;
    private float mushroomKnockbackTimer;

    protected override void Awake()
    {
        base.Awake(); 
    }

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        base.TakeDamage(dmg, hitDir);
        if (!isDead) mushroomKnockbackTimer = mushroomKnockbackDuration;
    }

    protected override void Update()
    {
        if (isDead) return;

        if (mushroomKnockbackTimer > 0)
        {
            mushroomKnockbackTimer -= Time.deltaTime;
            return; 
        }

        // Đã sửa lại ĐÚNG TÊN cục State trong Animator của bạn
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Mushroom_attack");

        // 1. Nếu đang bị Stun -> Đứng im, trừ giờ, không làm gì cả
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            SetVelocityX(0); 
            return; 
        }

        // 2. Nếu đang vung đòn đánh -> Đứng im chờ đánh xong (chống trượt)
        if (isAttacking)
        {
            SetVelocityX(0); 
            return; 
        }

        // 3. Hết Stun, hết Đánh -> Cho phép đuổi hoặc đánh tiếp
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
        UpdateAnimation(true); // Bật isMoving = true
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        int moveDir = directionToPlayer > 0 ? 1 : -1;
        
        if (moveDir != facingDir) Flip();
        
        SetVelocityX(moveSpeed * 1.5f * facingDir);
    }

    private void AttackLogic()
    {
        if (isDead) return; 
        SetVelocityX(0); 
        UpdateAnimation(false); // Tắt isMoving = false
        
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // Sửa lại tên ở đây nữa
            if (!isDead && !anim.GetCurrentAnimatorStateInfo(0).IsName("Mushroom_attack"))
            {
                anim.SetTrigger("Attack");
                lastAttackTime = Time.time; 
            }
        }
    }

    // Sự kiện gọi ở frame cuối cùng của đòn đánh
    public void TriggerStunAfterAttack()
    {
        stunTimer = stunDuration;
        anim.SetTrigger("Stun"); // Gọi đúng Trigger Stun như trong ảnh của bạn
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
                Debug.Log("Nấm đã đập trúng Player!");
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