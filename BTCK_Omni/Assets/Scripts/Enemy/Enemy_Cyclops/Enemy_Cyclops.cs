using UnityEngine;

public class Enemy_Cyclops : EnemyBase
{
    [Header("Cyclops AI Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.2f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private LayerMask targetLayer; // Layer của Player

    [Header("Stomp Attack (Cận chiến)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(2f, 1.5f);
    [SerializeField] private float stompDamage = 15f;

    [Header("Laser Eye Attack (Viễn chiến)")]
    [SerializeField] private CyclopsBeam laserPrefab; // Kéo Prefab CyclopsBeam vào đây
    [SerializeField] private Transform eyePoint;
    [SerializeField] private float laserDamage = 20f;
    [Tooltip("Dùng để AI biết nên dừng lại bắn ở khoảng cách bao xa")]
    [SerializeField] private float laserSightRange = 12f; 
    [Tooltip("Mask chặn tầm nhìn của quái (Cả Player và Tường)")]
    [SerializeField] private LayerMask laserSightMask; 

    private float lastAttackTime;

    // Tối ưu bộ nhớ cho chiêu Stomp
    private int hitBufferSize = 16;
    private Collider2D[] hitBuffer;

    // Hash để điều khiển Animator
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashStompState = Animator.StringToHash("Stomp");
    private readonly int hashLaserState = Animator.StringToHash("ShootLaser");
    private readonly int hashTriggerStomp = Animator.StringToHash("Attack"); 
    private readonly int hashTriggerLaser = Animator.StringToHash("ShootLaser");

    protected override void Awake()
    {
        base.Awake();
        hitBuffer = new Collider2D[hitBufferSize];
    }

    protected override void Update()
    {
        if (isDead) return;

        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.shortNameHash == hashHit || 
            stateInfo.shortNameHash == hashStompState || 
            stateInfo.shortNameHash == hashLaserState)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            return;
        }

        Transform target = GetVisiblePlayer();

        // [QUAN TRỌNG] Lọc lỗi 3: Kiểm tra target có tồn tại và đang "sống" không
        if (target != null && target.gameObject.activeInHierarchy)
        {
            isReturningHome = false;

            bool isPlayerInStompRange = false;
            if (attackPoint != null)
            {
                Collider2D hit = Physics2D.OverlapBox(attackPoint.position, attackSize, 0f, targetLayer);
                if (hit != null) isPlayerInStompRange = true;
            }

            if (isPlayerInStompRange)
            {
                SetVelocityX(0);
                anim.SetBool(animIsMoving, false);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    FaceTarget(target); 
                    PerformAttack(hashTriggerStomp);
                }
                return; 
            }
            
            Vector2 dirToPlayer = (target.position - eyePoint.position).normalized;
            float distToPlayer = Vector2.Distance(eyePoint.position, target.position);

            if (distToPlayer <= laserSightRange)
            {
                // [QUAN TRỌNG] Lọc lỗi 2: Bỏ qua collider của chính con Cyclops khi bắn tia Raycast
                // Mẹo: Tạm thời tắt collider của con quái đi để bắn tia, sau đó bật lại
                Collider2D myCollider = GetComponent<Collider2D>();
                if (myCollider != null) myCollider.enabled = false;

                RaycastHit2D sightTest = Physics2D.Raycast(eyePoint.position, dirToPlayer, laserSightRange, laserSightMask);
                
                if (myCollider != null) myCollider.enabled = true; // Bật lại ngay lập tức
                
                if (sightTest.collider != null && ((1 << sightTest.collider.gameObject.layer) & targetLayer) != 0)
                {
                    SetVelocityX(0);
                    anim.SetBool(animIsMoving, false);

                    if (Time.time >= lastAttackTime + attackCooldown)
                    {
                        FaceTarget(target);
                        PerformAttack(hashTriggerLaser);
                    }
                    return; 
                }
            }

            ChasePlayer(target);
            return;
        }

        if (isReturningHome)
        {
            ReturnHomeLogic();
            return;
        }

        float distToHome = Mathf.Abs(transform.position.x - startPosition.x);
        if (distToHome > (maxWanderDistance + 0.5f) && !isIdle)
        {
            isReturningHome = true;
        }

        if (isReturningHome) ReturnHomeLogic();
        else base.Update();
    }

    private void FaceTarget(Transform target)
    {
        float dirToPlayerX = target.position.x - transform.position.x;
        if ((dirToPlayerX > 0 && facingDir == -1) || (dirToPlayerX < 0 && facingDir == 1))
        {
            Flip(); 
        }
    }

    private void PerformAttack(int triggerHash)
    {
        lastAttackTime = Time.time;
        anim.SetTrigger(triggerHash);
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

    // ==========================================
    // ANIMATION EVENT: GỌI LÚC ĐẬP TAY XUỐNG
    // ==========================================
    public void ExecuteStompHit()
    {
        if (attackPoint == null) return;

        int hitCount = Physics2D.OverlapBoxNonAlloc(attackPoint.position, attackSize, 0f, hitBuffer, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit == null) continue;

            var entity = hit.GetComponent<Entity>();
            if (entity == null) continue;

            Vector2 knockDir = new Vector2(facingDir, 1f).normalized; 
            entity.TakeDamage(stompDamage, knockDir);
        }
    }
    public void ShootLaser_AnimEvent()
    {
        if (laserPrefab == null || eyePoint == null) return;
        CyclopsBeam beam = Instantiate(laserPrefab, eyePoint.position, Quaternion.identity);
        beam.Setup(facingDir, laserDamage);
    }

    // ==========================================
    // VẼ HỘP VÀ TIA NHÌN
    // ==========================================
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackSize);
        }

        if (eyePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + new Vector3(laserSightRange * (facingDir != 0 ? facingDir : 1), 0, 0));
        }
    }
}