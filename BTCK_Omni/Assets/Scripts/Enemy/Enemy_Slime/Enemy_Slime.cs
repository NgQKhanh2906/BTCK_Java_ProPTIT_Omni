using UnityEngine;

public class Enemy_Slime : EnemyBase
{
    [Header("Slime AI & Combat Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.5f; 
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private LayerMask targetLayer; 
    [Space]
    [Header("Hitbox & Damage")]
    [Tooltip("Vùng quét hình chữ nhật: Quyết định tầm đánh và phạm vi sát thương")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(1.2f, 0.8f);
    [SerializeField] private float attackDamage = 5f;

    private float lastAttackTime;
    private int hitBufferSize = 16;
    private Collider2D[] hitBuffer;
    private readonly int hashAttackState = Animator.StringToHash("Attack");
    private readonly int hashHitState = Animator.StringToHash("Hit");
    private readonly int hashAttackTrigger = Animator.StringToHash("Attack");

    protected override void Awake()
    {
        base.Awake();
        hitBuffer = new Collider2D[Mathf.Max(1, hitBufferSize)];
    }
    protected override void Update()
    {
        if (isDead) return;
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash == hashHitState)
        {
            anim.SetBool(animIsMoving, false);
            return;
        }
        if (stateInfo.shortNameHash == hashAttackState)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            return;
        }
        Transform target = GetVisiblePlayer();

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
                SetVelocityX(0);
                anim.SetBool(animIsMoving, false);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    lastAttackTime = Time.time;
                    anim.SetTrigger(hashAttackTrigger); 
                }
            }
            else
            {
                ChasePlayer(target);
            }
            return;
        }
        float distToHome = Mathf.Abs(transform.position.x - startPosition.x);
        if (distToHome > (maxWanderDistance + 0.5f) && !isIdle)
        {
            isReturningHome = true;
        }
        base.Update();
    }
    public void ExecuteAttackHit()
    {
        if (attackPoint == null) return;

        int hitCount = Physics2D.OverlapBoxNonAlloc(attackPoint.position, attackSize, 0f, hitBuffer, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit == null) continue;

            if (hit.TryGetComponent(out IDamageable damageable))
            {
                Vector2 hitDir = new Vector2(facingDir, 0.5f).normalized; 
                damageable.TakeDamage(attackDamage, hitDir);
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

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackSize);
        }
    }
}