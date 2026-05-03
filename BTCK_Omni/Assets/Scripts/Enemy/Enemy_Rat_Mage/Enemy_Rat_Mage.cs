using UnityEngine;

public class Enemy_Rat_Mage : EnemyBase
{
    [Header("Rat Mage AI Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.0f; 
    [SerializeField] private LayerMask targetLayer; // Layer của Player

    [Space]
    [Header("Melee Attack (Cận chiến)")]
    [SerializeField] private Transform meleeAttackPoint;
    [SerializeField] private Vector2 meleeAttackSize = new Vector2(1.2f, 1.2f);
    [SerializeField] private float meleeAttackDamage = 10f;
    [SerializeField] private float meleeAttackCooldown = 1.5f;
    private float lastMeleeTime;

    [Space]
    [Header("Spell Cast (Bắn băng)")]
    [SerializeField] private IceCone iceConePrefab; // Kéo Prefab IceCone vào đây
    [SerializeField] private Transform spellSpawnPoint; // Vị trí đầu gậy
    [SerializeField] private float spellDamage = 15f;
    [SerializeField] private float castRange = 8f; 
    [SerializeField] private float spellCooldown = 3.0f;
    [Tooltip("Mask chặn tầm nhìn của quái (Gồm cả Player và Tường/Đất)")]
    [SerializeField] private LayerMask spellSightMask; 
    private float lastSpellTime;

    // Tối ưu bộ nhớ
    private int hitBufferSize = 16;
    private Collider2D[] hitBuffer;

    // Hashes (Giữ nguyên vì tên State trong Animator của bạn không đổi)
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashCastSpell = Animator.StringToHash("CastSpell");

    protected override void Awake()
    {
        base.Awake();
        hitBuffer = new Collider2D[hitBufferSize];
    }

    protected override void Update()
    {
        if (isDead) return;

        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // 1. Đang bị thương hoặc đang thực hiện đòn đánh (Cận chiến / Bắn phép) thì phải ĐỨNG YÊN
        if (stateInfo.shortNameHash == hashHit || 
            stateInfo.shortNameHash == hashAttack || 
            stateInfo.shortNameHash == hashCastSpell)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            return;
        }

        Transform target = GetVisiblePlayer();

        // 2. Logic AI khi phát hiện Player
        if (target != null && target.gameObject.activeInHierarchy)
        {
            isReturningHome = false;

            // Kiểm tra xem Player có nằm trong vùng cận chiến không
            bool isPlayerInMeleeRange = false;
            if (meleeAttackPoint != null)
            {
                Collider2D hit = Physics2D.OverlapBox(meleeAttackPoint.position, meleeAttackSize, 0f, targetLayer);
                if (hit != null) isPlayerInMeleeRange = true;
            }

            // ƯU TIÊN 1: CẬN CHIẾN (Nếu Player áp sát)
            if (isPlayerInMeleeRange)
            {
                SetVelocityX(0);
                anim.SetBool(animIsMoving, false);

                if (Time.time >= lastMeleeTime + meleeAttackCooldown)
                {
                    FaceTarget(target);
                    lastMeleeTime = Time.time;
                    anim.SetTrigger(hashAttack);
                }
                return;
            }
            
            // ƯU TIÊN 2: BẮN PHÉP (Kiểm tra tia Raycast xem có bị tường cản không)
            Vector2 dirToPlayer = (target.position - spellSpawnPoint.position).normalized;
            float distToPlayer = Vector2.Distance(spellSpawnPoint.position, target.position);

            if (distToPlayer <= castRange)
            {
                // Tạm tắt Collider của chính quái vật để bắn tia Raycast không bị vướng vào mình
                Collider2D myCollider = GetComponent<Collider2D>();
                if (myCollider != null) myCollider.enabled = false;

                RaycastHit2D sightTest = Physics2D.Raycast(spellSpawnPoint.position, dirToPlayer, castRange, spellSightMask);
                
                if (myCollider != null) myCollider.enabled = true; // Bật lại ngay lập tức
                
                // Nếu tia Raycast chạm trúng đúng layer của Player
                if (sightTest.collider != null && ((1 << sightTest.collider.gameObject.layer) & targetLayer) != 0)
                {
                    SetVelocityX(0);
                    anim.SetBool(animIsMoving, false);

                    if (Time.time >= lastSpellTime + spellCooldown)
                    {
                        FaceTarget(target);
                        lastSpellTime = Time.time;
                        // Gọi Trigger "CastSpell" (Điều kiện trong Transition của bạn)
                        anim.SetTrigger(hashCastSpell); 
                    }
                    return; 
                }
            }

            // ƯU TIÊN 3: RƯỢT ĐUỔI (Nằm ngoài cả tầm cận chiến và tầm bắn phép)
            ChasePlayer(target);
            return;
        }

        // 3. Logic quay về vị trí cũ khi mất mục tiêu
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

    // Hàm xoay mặt về phía mục tiêu
    private void FaceTarget(Transform target)
    {
        float dirToPlayerX = target.position.x - transform.position.x;
        if ((dirToPlayerX > 0 && facingDir == -1) || (dirToPlayerX < 0 && facingDir == 1))
        {
            Flip();
        }
    }

    // ======================================================
    // ANIMATION EVENTS
    // ======================================================

    // Gắn vào Animation "Attack" (Đánh gần)
    public void ExecuteMeleeAttackHit()
    {
        if (meleeAttackPoint == null) return;
        int hitCount = Physics2D.OverlapBoxNonAlloc(meleeAttackPoint.position, meleeAttackSize, 0f, hitBuffer, targetLayer);
        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit == null) continue;

            var entity = hit.GetComponent<Entity>();
            if (entity != null)
            {
                // Lực hất lùi nhẹ
                Vector2 hitDir = new Vector2(facingDir, 0.2f).normalized; 
                entity.TakeDamage(meleeAttackDamage, hitDir);
            }
        }
    }

    // Gắn vào Animation "CastSpell" (Lúc giơ gậy vẩy băng ra)
    public void ExecuteCastSpell()
    {
        if (iceConePrefab == null || spellSpawnPoint == null) return;

        // Sinh ra cục băng IceCone và gọi hàm Setup để truyền hướng + sát thương
        IceCone iceCone = Instantiate(iceConePrefab, spellSpawnPoint.position, Quaternion.identity);
        iceCone.Setup(facingDir, spellDamage);
    }

    // ======================================================
    // LOGIC RƯỢT ĐUỔI & GIZMOS
    // ======================================================

    protected override void ChasePlayer(Transform target)
    {
        bool isLedgeAhead = ledgeCheck != null && ledgeCheck.IsDetectingLedge();
        bool isWallAhead = IsWallDetected();

        // Thấy tường hoặc vực thì đứng im nhìn Player
        if (isWallAhead || isLedgeAhead)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            FaceTarget(target);
        }
        // Đường thoáng thì tiếp tục chạy
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

        if (meleeAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(meleeAttackPoint.position, meleeAttackSize);
        }

        if (spellSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(spellSpawnPoint.position, spellSpawnPoint.position + new Vector3(castRange * (facingDir != 0 ? facingDir : 1), 0, 0));
        }
    }
}