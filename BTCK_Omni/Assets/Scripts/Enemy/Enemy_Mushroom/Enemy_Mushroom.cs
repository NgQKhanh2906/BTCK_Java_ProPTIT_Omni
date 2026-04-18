using UnityEngine;

[RequireComponent(typeof(MeleeAttack))]
[RequireComponent(typeof(PlayerDetector))] 
public class Enemy_Mushroom : EnemyBase
{
    [Header("Mushroom AI Settings")]
    [SerializeField] private float attackRange = 1.5f; 
    [SerializeField] private float chaseSpeedMultiplier = 1.5f;

    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 2f; 
    private float stunTimer;

    private MeleeAttack meleeWeapon;
    private PlayerDetector eyes;

    protected override void Awake()
    {
        base.Awake();
        meleeWeapon = GetComponent<MeleeAttack>();
        eyes = GetComponent<PlayerDetector>();
    }
    protected override void Update()
    {
        if (isDead) return;
        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName(GameConfig.ANIM_COL_HIT))
        {
            return; 
        }
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            SetVelocityX(0);
            return; 
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(GameConfig.ANIM_COL_ATTACK) || 
            anim.GetCurrentAnimatorStateInfo(0).IsName("Mushroom_attack"))
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
    public void TriggerStunAfterAttack()
    {
        stunTimer = stunDuration;
        if (anim != null) anim.SetTrigger(animStun); 
        
        Debug.Log("Mushroom: Đánh xong mệt quá, xin nghỉ mệt 2 giây!");
    }
}