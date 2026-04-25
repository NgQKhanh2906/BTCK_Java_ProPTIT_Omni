using UnityEngine;

public class Enemy_Minotaur : EnemyBase
{
    [Header("Minotaur AI Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.2f; 
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private LayerMask targetLayer; 

    [Header("Combat & Range (Attack 1 - Hộp Đỏ)")] 
    [SerializeField] private Transform attackPoint1;
    [SerializeField] private Vector2 size1;
    [SerializeField] private float damage1 = 10f;

    [Header("Combat & Range (Attack 2 - Hộp Xanh)")]
    [SerializeField] private Transform attackPoint2;
    [SerializeField] private Vector2 size2;
    [SerializeField] private float damage2 = 20f;

    private int hitBufferSize = 16;
    private Collider2D[] hitBuffer;

    private float lastAttackTime;
    private int comboStep = 0; 
    
    private readonly int hashAttack1 = Animator.StringToHash("Attack1");
    private readonly int hashAttack2 = Animator.StringToHash("Attack2");
    private readonly int hashHit = Animator.StringToHash("Hit");

    protected override void Awake()
    {
        base.Awake();
        hitBuffer = new Collider2D[Mathf.Max(1, hitBufferSize)];
    }

    protected override void Update()
    {
        if (isDead) return;

        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // 1. KHÓA TRẠNG THÁI: Đang bị choáng (Hit)
        // (Bỏ SetVelocityX(0) để Minotaur có thể bị Player đánh bật lùi về sau)
        if (stateInfo.shortNameHash == hashHit)
        {
            anim.SetBool(animIsMoving, false);
            return;
        }

        // 2. KHÓA TRẠNG THÁI: Đang vung rìu
        if (stateInfo.shortNameHash == hashAttack1 || stateInfo.shortNameHash == hashAttack2)
        {
            SetVelocityX(0); 
            anim.SetBool(animIsMoving, false);
            return; 
        }

        Transform target = GetVisiblePlayer();

        if (target != null)
        {
            isReturningHome = false; 

            // 3. AI QUÉT MỤC TIÊU BẰNG HITBOX (MẮT = TAY)
            bool isPlayerInAttackRange = false;
            
            // Tự động chọn hộp quét dựa trên đòn đánh tiếp theo trong Combo
            Transform currentPoint = (comboStep % 2 == 0) ? attackPoint1 : attackPoint2;
            Vector2 currentSize = (comboStep % 2 == 0) ? size1 : size2;

            if (currentPoint != null)
            {
                Collider2D hit = Physics2D.OverlapBox(currentPoint.position, currentSize, 0f, targetLayer);
                if (hit != null) isPlayerInAttackRange = true;
            }

            // Nếu lọt vào hộp quét -> Phanh lại và Combo
            if (isPlayerInAttackRange)
            {
                SetVelocityX(0); 
                anim.SetBool(animIsMoving, false); 
                TryExecuteCombo(); 
            }
            // Nếu chưa lọt vào hộp quét -> Tiếp tục rượt đuổi
            else 
            {
                ChasePlayer(target);
            }
            return; 
        }

        // 4. KIỂM TRA ĐIỀU KIỆN VỀ Ổ
        if (isReturningHome)
        {
            ReturnHomeLogic();
            return;
        }

        float distToHome = Mathf.Abs(transform.position.x - startPosition.x);
        if (distToHome > maxWanderDistance && !isIdle)
        {
            isReturningHome = true;
        }

        if (isReturningHome)
        {
            ReturnHomeLogic();
            return;
        }

        // Đi tuần tra mặc định
        base.Update();
    }

    private void TryExecuteCombo()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            comboStep++;
            
            if (comboStep % 2 != 0)
            {
                anim.SetTrigger(hashAttack1);
            }
            else
            {
                anim.SetTrigger(hashAttack2);
            }
        }
    }

    #region Animation Events For Attack
    public void Hit1()
    {
        OnAttackHit(attackPoint1, size1, damage1);
    }
    
    public void Hit2()
    {
        OnAttackHit(attackPoint2, size2, damage2);
    }
    #endregion

    public void OnAttackHit(Transform attackPoint, Vector2 size, float dmg)
    {
        if (attackPoint == null) return;
        
        int hitCount = Physics2D.OverlapBoxNonAlloc(attackPoint.position, size, 0f, hitBuffer, targetLayer);
        
        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit == null) continue;
            
            if (hit.TryGetComponent(out IDamageable damageable))
            {
                // Lực hất tiêu chuẩn: Hướng mặt quái và chếch lên trên 0.5f
                Vector2 dir = new Vector2(facingDir, 0.5f).normalized; 
                damageable.TakeDamage(dmg, dir);
            }
        }
    }

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

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Vẽ Hộp Đỏ (Attack 1)
        Gizmos.color = Color.red;
        if (attackPoint1 != null)
        {
            Gizmos.DrawWireCube(attackPoint1.position, size1);
        }

        // Vẽ Hộp Xanh Lá (Attack 2)
        Gizmos.color = Color.green;
        if (attackPoint2 != null)
        {
            Gizmos.DrawWireCube(attackPoint2.position, size2);
        }
    }
}