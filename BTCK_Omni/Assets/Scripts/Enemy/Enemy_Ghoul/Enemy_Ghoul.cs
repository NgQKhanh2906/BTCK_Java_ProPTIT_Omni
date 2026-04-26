using UnityEngine;

public class Enemy_Ghoul : EnemyBase
{
    [Header("Ghoul AI & Combat Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.5f; 
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private LayerMask targetLayer; // Layer của Player

    [Space]
    [Tooltip("Vùng này vừa dùng để AI quyết định dừng lại chém, vừa là vùng gây sát thương")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(1.5f, 1.2f);
    [SerializeField] private float attackDamage = 10f;

    private float lastAttackTime;

    // Tối ưu bộ nhớ (NonAlloc) để tránh tạo rác (GC)
    private int hitBufferSize = 16;
    private Collider2D[] hitBuffer;

    // Các mã Hash để Animator chạy nhanh hơn
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHit = Animator.StringToHash("Hit");

    protected override void Awake()
    {
        base.Awake();
        // Khởi tạo mảng đệm quét va chạm
        hitBuffer = new Collider2D[hitBufferSize];
    }

    protected override void Update()
    {
        if (isDead) return;

        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // 1. Trạng thái bị thương (Hit):
        // Không gọi SetVelocityX(0) để lực đẩy lùi (Knockback) từ Entity.cs hoạt động tự nhiên
        if (stateInfo.shortNameHash == hashHit)
        {
            anim.SetBool(animIsMoving, false);
            return;
        }

        // 2. Trạng thái đang tấn công (Attack):
        // Phải đứng yên tại chỗ khi vung tay cào
        if (stateInfo.shortNameHash == hashAttack)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            return;
        }

        // 3. Logic AI tìm kiếm Player
        Transform target = GetVisiblePlayer();

        if (target != null)
        {
            isReturningHome = false;

            // KIỂM TRA TẦM ĐÁNH (Gộp chung với Hitbox)
            bool isPlayerInAttackRange = false;
            if (attackPoint != null)
            {
                // Quét hình hộp xem Player có lọt vào vùng này không
                Collider2D hit = Physics2D.OverlapBox(attackPoint.position, attackSize, 0f, targetLayer);
                if (hit != null) isPlayerInAttackRange = true;
            }

            if (isPlayerInAttackRange)
            {
                // Đã vào tầm: Dừng lại và chém (nếu hết cooldown)
                SetVelocityX(0);
                anim.SetBool(animIsMoving, false);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    lastAttackTime = Time.time;
                    anim.SetTrigger(hashAttack);
                }
            }
            else
            {
                // Chưa vào tầm: Tiếp tục rượt đuổi
                ChasePlayer(target);
            }
            return;
        }

        // 4. Khi không thấy Player: Quay về vị trí ban đầu
        float distToHome = Mathf.Abs(transform.position.x - startPosition.x);
        if (distToHome > (maxWanderDistance + 0.5f) && !isIdle)
        {
            isReturningHome = true;
        }

        base.Update();
    }

    // ======================================================
    // ANIMATION EVENT: Gắn vào frame vung tay của Ghoul_attack
    // ======================================================
    public void ExecuteAttackHit()
    {
        if (attackPoint == null) return;

        // Quét sát thương bằng NonAlloc để tối ưu hiệu năng
        int hitCount = Physics2D.OverlapBoxNonAlloc(attackPoint.position, attackSize, 0f, hitBuffer, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit == null) continue;

            var entity = hit.GetComponent<Entity>();
            if (entity == null) continue;

            // Truyền hướng hất nhẹ (normalized) để lớp cha Entity tự xử lý Knockback
            Vector2 hitDir = new Vector2(facingDir, 0.2f).normalized; 
            entity.TakeDamage(attackDamage, hitDir);
        }
    }

    // Logic rượt đuổi Player
    protected override void ChasePlayer(Transform target)
    {
        bool isLedgeAhead = ledgeCheck != null && ledgeCheck.IsDetectingLedge();
        bool isWallAhead = IsWallDetected();

        if (isWallAhead || isLedgeAhead)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            
            float dirToPlayerX = target.position.x - transform.position.x;
            if ((dirToPlayerX > 0 && facingDir == -1) || (dirToPlayerX < 0 && facingDir == 1))
            {
                Flip();
            }
        }
        else
        {
            isIdle = false;
            anim.SetBool(animIsMoving, true);
            
            float dirToPlayerX = target.position.x - transform.position.x;
            int moveDir = dirToPlayerX > 0 ? 1 : -1;

            if (moveDir != facingDir) Flip();

            SetVelocityX(moveSpeed * chaseSpeedMultiplier * facingDir);
        }
    }

    // Vẽ hình chữ nhật đỏ để căn chỉnh vùng "Mắt & Tay" trong Unity Editor
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackSize);
        }
    }
}