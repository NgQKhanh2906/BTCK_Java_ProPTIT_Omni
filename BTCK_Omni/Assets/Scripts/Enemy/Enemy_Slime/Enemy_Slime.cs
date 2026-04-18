using UnityEngine;

[RequireComponent(typeof(MeleeAttack))]
public class Enemy_Slime : EnemyBase
{
    [Header("Slime AI Settings")]
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private float chaseSpeedMultiplier = 1.5f;
    private MeleeAttack meleeWeapon;

    protected override void Awake()
    {
        base.Awake(); 
        meleeWeapon = GetComponent<MeleeAttack>();
    }
    protected override void Update()
    {
        if (isDead) return;
        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName(GameConfig.ANIM_COL_HIT))
        {
            return;
        }
        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("Slime_attack"))
        {
            SetVelocityX(0);
            return;
        }
        Transform target = GetVisiblePlayer();

        if (target != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            
            if (distanceToPlayer <= attackRange)
            {
                SetVelocityX(0); 
                anim.SetBool(animIsMoving, false); 
                meleeWeapon.TryAttack(); 
                return; 
            }
            else
            {
                ChasePlayer(target);
                return;
            }
        }
        base.Update(); 
    }

    private void ChasePlayer(Transform target)
    {
        bool isLedgeAhead = ledgeCheck != null && ledgeCheck.IsDetectingLedge();
        bool isWallAhead = IsWallDetected();

        if (isWallAhead || isLedgeAhead)
        {
            SetVelocityX(0); 
            anim.SetBool(animIsMoving, false); 
            
            float dirToPlayer = target.position.x - transform.position.x;
            if ((dirToPlayer > 0 && facingDir == -1) || (dirToPlayer < 0 && facingDir == 1))
            {
                Flip();
            }
        }
        else 
        {
            isIdle = false; 
            anim.SetBool(animIsMoving, true);

            float directionToPlayer = target.position.x - transform.position.x;
            int moveDir = directionToPlayer > 0 ? 1 : -1;
            
            if (moveDir != facingDir) Flip();
            
            SetVelocityX(moveSpeed * chaseSpeedMultiplier * facingDir);
        }
    }
}