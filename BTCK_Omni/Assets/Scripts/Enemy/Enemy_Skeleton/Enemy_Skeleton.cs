using UnityEngine;
public class Enemy_Skeleton : EnemyBase
{
    [Header("Skeleton AI Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.8f; 
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Combat & Range (Gộp Mắt và Tay)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(1.2f, 1.5f);
    [SerializeField] private float attackDamage = 10f;

    private float lastAttackTime;
    private int hitBufferSize = 16;
    private Collider2D[] hitBuffer;
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int paramIsChasing = Animator.StringToHash("isChasing");

    protected override void Awake()
    {
        base.Awake();
        hitBuffer = new Collider2D[Mathf.Max(1, hitBufferSize)];
    }

    protected override void Update()
    {
        if (isDead) return;
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash == hashHit)
        {
            anim.SetBool(animIsMoving, false);
            anim.SetBool(paramIsChasing, false);
            return;
        }
        if (stateInfo.shortNameHash == hashAttack)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            anim.SetBool(paramIsChasing, false);
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
                anim.SetBool(paramIsChasing, false);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    lastAttackTime = Time.time;
                    anim.SetTrigger(hashAttack);
                }
            }
            else
            {
                ChasePlayer(target);
            }
            return; 
        }
        anim.SetBool(paramIsChasing, false);
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

        base.Update(); 
    }

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
            anim.SetBool(animIsMoving, false);
            anim.SetBool(paramIsChasing, true);

            float dirToPlayerX = target.position.x - transform.position.x;
            int moveDir = dirToPlayerX > 0 ? 1 : -1;

            if (moveDir != facingDir) Flip();

            SetVelocityX(moveSpeed * chaseSpeedMultiplier * facingDir);
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
            Vector2 hitDir = new Vector2(facingDir, 0.5f).normalized;
            entity.TakeDamage(attackDamage, hitDir);
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.red;
        if (attackPoint != null)
        {
            Gizmos.DrawWireCube(attackPoint.position, attackSize);
        }
    }
}