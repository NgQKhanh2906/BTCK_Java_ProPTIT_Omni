using UnityEngine;

public class Enemy_Wolf : EnemyBase
{
    [Header("Detection & Attack Range")]
    [SerializeField] private bool playerInDetectionRange;
    [SerializeField] private bool playerInAttackRange;
    private Transform playerTransform;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCheckRadius = 5f;
    [SerializeField] private Transform attackCheckPoint; // Kéo Empty Object ở mồm vào đây
    [SerializeField] private LayerMask whatIsPlayer;    // Chọn Layer Player
    [SerializeField] private float attackCooldown = 1.5f;    // Độ trễ giữa 2 lần đánh
    private float lastAttackTime;

    public DamageComponent damageComponent;

    protected virtual void Awake()
    {
        base.Awake();
        damageComponent = new DamageComponent(attackDamage);

    }

    protected override void Update()
    {
        if (isDead) return;

        // ƯU TIÊN 1: Tấn công nếu trong tầm đánh
        if (playerInAttackRange)
        {
            AttackLogic();
        }
        // ƯU TIÊN 2: Đuổi theo nếu thấy Player
        else if (playerInDetectionRange && playerTransform != null)
        {
            ChaseLogic();
        }
        // ƯU TIÊN 3: Đi tuần (Patrol) mặc định từ lớp cha
        else
        {
            base.Update();
        }
    }

    private void ChaseLogic()
    {
        isAdle = false; // Đảm bảo không bị kẹt ở trạng thái nghỉ
        UpdateAnimation(true); // Bật anim chạy

        // Tính hướng di chuyển dựa trên vị trí Player
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        int moveDir = directionToPlayer > 0 ? 1 : -1;

        // Chỉ Flip khi cần thiết để tránh giật lag
        if (moveDir != facingDir)
        {
            Flip();
        }

        // Đuổi theo với tốc độ nhanh hơn đi tuần (nhân 1.5)
        SetVelocityX(moveSpeed * 1.5f * facingDir);
    }

    private void AttackLogic()
    {
        SetVelocityX(0); // Dừng lại để đánh
        UpdateAnimation(false);
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Enemy_Wolf_Attack"))
            {
                anim.SetTrigger("Attack");
                lastAttackTime = Time.time; // Cập nhật lại mốc thời gian vừa đánh
                AnimationAttackTrigger();
            }
        }
    }

    public void AnimationAttackTrigger()
    {
        if (attackCheckPoint == null) return;

        Collider2D hitPlayer = Physics2D.OverlapCircle(attackCheckPoint.position, attackCheckRadius, whatIsPlayer);

        if (hitPlayer != null)
        {
            if (hitPlayer.TryGetComponent(out ITakeDamageable target))
            {
                damageComponent.Attack(target);
                Debug.Log("Đã gây dame cho Player!");
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