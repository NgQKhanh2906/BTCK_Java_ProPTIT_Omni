using UnityEngine;

[RequireComponent(typeof(MeleeAttack))]
public class Enemy_Wolf : EnemyBase
{
    [Header("Wolf AI Settings")]
    [SerializeField] private float attackRange = 1.5f; 
    [SerializeField] private float chaseSpeedMultiplier = 1.5f; // Tốc độ rượt sẽ nhanh hơn đi bộ

    private MeleeAttack meleeWeapon;

    protected override void Awake()
    {
        base.Awake();
        meleeWeapon = GetComponent<MeleeAttack>();
    }

    protected override void Update()
    {
        if (isDead) return;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Wolf_attack"))
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
}