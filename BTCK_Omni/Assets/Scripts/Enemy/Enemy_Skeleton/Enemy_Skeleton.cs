using UnityEngine;

[RequireComponent(typeof(MeleeAttack))]
public class Enemy_Skeleton : EnemyBase
{
    [Header("Skeleton AI Settings")]
    [SerializeField] private float attackRange = 1.2f; 
    [SerializeField] private float chaseSpeedMultiplier = 1.8f; // Skeleton chạy nhanh hơn Sói lúc rượt đuổi
    
    private MeleeAttack meleeWeapon;

    // Các mã Hash cục bộ để khớp tuyệt đối với Animator của Skeleton
    private readonly int hashStateAttack = Animator.StringToHash("Attack");
    private readonly int hashStateHit = Animator.StringToHash("Hit");
    private readonly int paramIsChasing = Animator.StringToHash("isChasing");

    protected override void Awake()
    {
        base.Awake(); 
        meleeWeapon = GetComponent<MeleeAttack>();
    }

    protected override void Update()
    {
        if (isDead) return;

        // 1. KIỂM TRA TRẠNG THÁI HIỆN TẠI (Hit, Attack)
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.shortNameHash == hashStateHit || stateInfo.shortNameHash == hashStateAttack)
        {
            SetVelocityX(0); 
            // Đảm bảo mọi cờ di chuyển bị tắt để Skeleton không trượt patin khi đang chém/bị đánh
            anim.SetBool(animIsMoving, false);
            anim.SetBool(paramIsChasing, false);
            return; 
        }

        // 2. RADAR TÌM NGƯỜI
        Transform target = GetVisiblePlayer();

        // 3. NẾU THẤY NGƯỜI CHƠI -> CHIẾM QUYỀN LỚP CHA
        if (target != null)
        {
            isReturningHome = false; 

            float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            if (distanceToPlayer <= attackRange)
            {
                SetVelocityX(0); 
                // Tắt trạng thái chạy để chuẩn bị chém
                anim.SetBool(animIsMoving, false); 
                anim.SetBool(paramIsChasing, false); 

                // Component MeleeAttack sẽ tự động kích hoạt Trigger "Attack" (lấy từ GameConfig)
                meleeWeapon.TryAttack(); 
            }
            else 
            {
                ChasePlayer(target);
            }
            return; 
        }

        // 4. NẾU MẤT DẤU -> TRẢ QUYỀN CHO LỚP CHA
        // Tắt ngay cờ đuổi để khi lớp cha gọi UpdateAnimation(true), nó sẽ kích hoạt đúng trạng thái Walk
        anim.SetBool(paramIsChasing, false);

        float distToHome = Mathf.Abs(transform.position.x - startPosition.x);
        if (distToHome > (maxWanderDistance + 0.5f) && !isIdle)
        {
            isReturningHome = true;
        }

        // Ủy quyền lại cho EnemyBase lo việc đi tuần (Walk) và đứng thở (Idle)
        base.Update();
    }

    // Ghi đè (Override) hoàn toàn hành vi đuổi của lớp cha
    protected override void ChasePlayer(Transform target)
    {
        bool isLedgeAhead = ledgeCheck != null && ledgeCheck.IsDetectingLedge();
        bool isWallAhead = IsWallDetected();

        if (isWallAhead || isLedgeAhead)
        {
            SetVelocityX(0); 
            anim.SetBool(animIsMoving, false); 
            anim.SetBool(paramIsChasing, false);
            
            float dirToPlayerX = target.position.x - transform.position.x;
            if ((dirToPlayerX > 0 && facingDir == -1) || (dirToPlayerX < 0 && facingDir == 1))
            {
                Flip(); 
            }
        }
        else 
        {
            isIdle = false; 
            
            // TUYỆT CHIÊU DÀNH CHO ANIMATOR:
            // Ép isMoving = false và isChasing = true để luồng mũi tên nhảy vào đúng ô "Chasing"
            anim.SetBool(animIsMoving, false);
            anim.SetBool(paramIsChasing, true);
            
            float dirToPlayerX = target.position.x - transform.position.x;
            int moveDir = dirToPlayerX > 0 ? 1 : -1;
            
            if (moveDir != facingDir) Flip();
            
            SetVelocityX(moveSpeed * chaseSpeedMultiplier * facingDir);
        }
    }
}