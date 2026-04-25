using UnityEngine;

public class Enemy_Flying_Eye : EnemyBase
{
    [Header("Flying AI Settings")]
    [SerializeField] private float flySpeed = 4f; 
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Combat & Range (Gộp Mắt và Tay)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(1.5f, 1.5f);
    [SerializeField] private float attackDamage = 5f;

    private float lastAttackTime;
    private int hitBufferSize = 16;
    private Collider2D[] hitBuffer;

    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHit = Animator.StringToHash("Hit");

    protected override void Awake()
    {
        base.Awake();
        hitBuffer = new Collider2D[hitBufferSize];
        
        // Tắt trọng lực để quái bay không rớt
        if (rb != null) rb.gravityScale = 0f;
    }

    protected override void Update()
    {
        if (isDead) return;

        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.shortNameHash == hashHit)
        {
            anim.SetBool(animIsMoving, false);
            return;
        }

        if (stateInfo.shortNameHash == hashAttack)
        {
            rb.velocity = Vector2.zero;
            anim.SetBool(animIsMoving, false);
            return;
        }

        Transform target = GetVisiblePlayer();

        // NẾU THẤY PLAYER -> LAO VÀO RƯỢT THEO ĐƯỜNG CHÉO
        if (target != null)
        {
            isReturningHome = false;

            bool isPlayerInAttackRange = false;
            if (attackPoint != null)
            {
                Collider2D hit = Physics2D.OverlapBox(attackPoint.position, attackSize, 0f, targetLayer);
                if (hit != null) isPlayerInAttackRange = true;
            }

            if (isPlayerInAttackRange)
            {
                rb.velocity = Vector2.zero; 
                anim.SetBool(animIsMoving, false);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    lastAttackTime = Time.time;
                    anim.SetTrigger(hashAttack);
                }
            }
            else
            {
                ChasePlayerFly(target.position);
            }
            return; 
        }

        // ====================================================
        // NẾU KHÔNG THẤY PLAYER -> BAY TUẦN TRA QUANH Ổ
        // ====================================================
        
        // Kiểm tra xem có bị bay đi quá xa ổ không?
        float distToHome = Vector2.Distance(transform.position, startPosition);
        if (distToHome > maxWanderDistance + 1f)
        {
            isReturningHome = true;
        }

        // Lựa chọn trạng thái lúc nhàn rỗi
        if (isReturningHome)
        {
            ReturnHomeFly();
        }
        else
        {
            if (isIdle) HandleIdle(); // Đứng yên tại chỗ 1 lúc (dùng chung của EnemyBase)
            else HandlePatrol();      // Bay qua bay lại (Đã được Ghi đè ở dưới)
        }
    }

    // ===================================================
    // GHI ĐÈ LẠI HỆ THỐNG TUẦN TRA DÀNH RIÊNG CHO QUÁI BAY
    // ===================================================

    // Khi đi tuần tra, quái bay CHỈ cần quan tâm đụng tường, không sợ rớt hố (Ledge)
    protected override void HandlePatrol()
    {
        bool isWallAhead = IsWallDetected();
        bool reachedLeft = transform.position.x <= patrolBoundLeft && facingDir == -1;
        bool reachedRight = transform.position.x >= patrolBoundRight && facingDir == 1;

        if (isWallAhead || reachedLeft || reachedRight)
        {
            StartIdle(); // Tới giới hạn thì dừng lại nghỉ
        }
        else
        {
            // Bay tà tà theo phương ngang
            rb.velocity = new Vector2(flySpeed * facingDir, 0); 
            anim.SetBool(animIsMoving, true);
        }
    }

    // Khi nghỉ ngơi (Idle) trên không, phải ép vận tốc Y về 0 để không bị trôi
    protected override void StartIdle()
    {
        isIdle = true;
        idleTimer = idleDuration; // Thời gian nghỉ lấy từ EnemyBase
        rb.velocity = Vector2.zero; 
        anim.SetBool(animIsMoving, false);
    }

    // ===================================================
    // HỆ THỐNG BAY RƯỢT ĐUỔI VÀ TÌM ĐƯỜNG VỀ
    // ===================================================
    private void ChasePlayerFly(Vector3 targetPos)
    {
        isIdle = false;
        anim.SetBool(animIsMoving, true);

        Vector2 dir = (targetPos - transform.position).normalized;
        
        if (IsWallDetected())
        {
            dir.x = 0;
            if (dir.y == 0) dir.y = 1f; 
            dir = dir.normalized;
        }

        rb.velocity = dir * flySpeed;

        if ((targetPos.x - transform.position.x > 0 && facingDir == -1) || 
            (targetPos.x - transform.position.x < 0 && facingDir == 1))
        {
            Flip();
        }
    }

    private void ReturnHomeFly()
    {
        // Khi bay gần tới ổ rồi thì hủy trạng thái Về nhà, chuyển sang trạng thái Nghỉ ngơi
        float distToHome = Vector2.Distance(transform.position, startPosition);
        if (distToHome < 0.5f)
        {
            isReturningHome = false;
            StartIdle();
            return;
        }

        isIdle = false;
        anim.SetBool(animIsMoving, true);

        Vector2 dir = ((Vector3)startPosition - transform.position).normalized;

        if (IsWallDetected())
        {
            dir.x = 0;
            if (dir.y == 0) dir.y = 1f;
            dir = dir.normalized;
        }

        rb.velocity = dir * flySpeed;

        if ((dir.x > 0 && facingDir == -1) || (dir.x < 0 && facingDir == 1))
        {
            Flip();
        }
    }

    public void ExecuteAttackHit()
    {
        if (attackPoint == null) return;

        int hitCount = Physics2D.OverlapBoxNonAlloc(attackPoint.position, attackSize, 0f, hitBuffer, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit == null) continue;

            var entity = hit.GetComponent<Entity>();
            if (entity == null) continue;

            Vector2 hitDir = (hit.transform.position - transform.position).normalized; 
            entity.TakeDamage(attackDamage, hitDir);
        }
    }

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