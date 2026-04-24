using UnityEngine;

[RequireComponent(typeof(MeleeAttack))]
public class Enemy_Wolf : EnemyBase
{
    [Header("Wolf AI Settings")]
    [SerializeField] private float attackRange = 1.5f; 
    [SerializeField] private float chaseSpeedMultiplier = 1.5f; 
    
    private MeleeAttack meleeWeapon;
    private readonly int hashWolfAttack = Animator.StringToHash("Attack");
    private int hashHit;

    protected override void Awake()
    {
        base.Awake(); 
        meleeWeapon = GetComponent<MeleeAttack>();
        hashHit = Animator.StringToHash(GameConfig.ANIM_COL_HIT);
    }

    protected override void Update()
    {
        if (isDead) return;
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash == hashHit) return; 
        
        if (stateInfo.shortNameHash == hashWolfAttack)
        {
            SetVelocityX(0); 
            return; 
        }
        Transform target = GetVisiblePlayer();

        if (target != null)
        {
            isReturningHome = false; 

            float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            if (distanceToPlayer <= attackRange)
            {
                SetVelocityX(0); 
                UpdateAnimation(false); 
                meleeWeapon.TryAttack(); 
            }
            else 
            {
                ChasePlayer(target);
            }
            return; 
        }
        float distToHome = Mathf.Abs(transform.position.x - startPosition.x);
        if (distToHome > maxWanderDistance && !isIdle)
        {
            isReturningHome = true;
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
            UpdateAnimation(false);
            
            float dirToPlayerX = target.position.x - transform.position.x;
            if ((dirToPlayerX > 0 && facingDir == -1) || (dirToPlayerX < 0 && facingDir == 1))
            {
                Flip(); 
            }
        }
        else 
        {
            isIdle = false; 
            UpdateAnimation(true);
            float dirToPlayerX = target.position.x - transform.position.x;
            int moveDir = dirToPlayerX > 0 ? 1 : -1;
            if (moveDir != facingDir) Flip();
            SetVelocityX(moveSpeed * chaseSpeedMultiplier * facingDir);
        }
    }
}