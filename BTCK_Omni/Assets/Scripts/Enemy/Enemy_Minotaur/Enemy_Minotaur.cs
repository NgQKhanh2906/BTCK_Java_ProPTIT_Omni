using UnityEngine;

public class Enemy_Minotaur : EnemyBase
{
    [Header("Minotaur AI Settings")]
    [SerializeField] private float attackRange = 2f; 
    [SerializeField] private float chaseSpeedMultiplier = 1.2f; 
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private LayerMask targetLayer; 

    [Header("Attack 1 Range")] 
    [SerializeField] private Transform attackPoint1;
    [SerializeField] private Vector2 size1;
    [SerializeField] private float damage1 = 10f;

    [Header("Attack 2 Range")]
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
        if (stateInfo.shortNameHash == hashHit)
        {
            anim.SetBool(animIsMoving, false);
            return;
        }
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
            float distanceToPlayerX = Mathf.Abs(transform.position.x - target.position.x);

            if (distanceToPlayerX <= attackRange)
            {
                SetVelocityX(0); 
                anim.SetBool(animIsMoving, false); 
                TryExecuteCombo(); 
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

    #region AnimationEventForAttack
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
        
        if (hitBuffer == null || hitBuffer.Length != hitBufferSize)
            hitBuffer = new Collider2D[hitBufferSize];
        int hitCount = Physics2D.OverlapBoxNonAlloc(attackPoint.position, size, 0f, hitBuffer, targetLayer);
        
        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit == null) continue;
            var entity = hit.GetComponent<Entity>();
            if (entity == null) continue;
            Vector2 dir = new Vector2(facingDir, 0.5f).normalized; 
            entity.TakeDamage(dmg, dir);
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
        Gizmos.color = Color.magenta;
        Vector3 leftBound = transform.position + new Vector3(-attackRange, 0, 0);
        Vector3 rightBound = transform.position + new Vector3(attackRange, 0, 0);
        Gizmos.DrawLine(leftBound + Vector3.up * 2f, leftBound + Vector3.down * 2f);
        Gizmos.DrawLine(rightBound + Vector3.up * 2f, rightBound + Vector3.down * 2f);
        Gizmos.color = Color.red;
        if (attackPoint1 != null)
        {
            Gizmos.DrawWireCube(attackPoint1.position, size1);
        }
        Gizmos.color = Color.green;
        if (attackPoint2 != null)
        {
            Gizmos.DrawWireCube(attackPoint2.position, size2);
        }
    }
}