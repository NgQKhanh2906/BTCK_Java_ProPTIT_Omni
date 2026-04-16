using UnityEngine;

// Tự động gắn Mắt và Vũ khí
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

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.2f;
    private float knockbackTimer;

    private MeleeAttack meleeWeapon;
    private PlayerDetector eyes;

    protected override void Awake()
    {
        base.Awake();
        meleeWeapon = GetComponent<MeleeAttack>();
        eyes = GetComponent<PlayerDetector>();
    }

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        base.TakeDamage(dmg, hitDir);
        if (!isDead) knockbackTimer = knockbackDuration;
    }

    protected override void Update()
    {
        if (isDead) return;
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            return; 
        }

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            SetVelocityX(0); 
            return; 
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Mushroom_attack"))
        {
            SetVelocityX(0); 
            return; 
        }
        if (eyes.CanSeePlayer())
        {
            Transform target = eyes.GetPlayerTransform();
            float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            if (distanceToPlayer <= attackRange)
            {
                SetVelocityX(0);
                anim.SetBool(animIsMoving, false);
                meleeWeapon.TryAttack();
            }
            else
            {
                ChasePlayer(target);
            }
        }
        else
        {
            base.Update();
        }
    }

    private void ChasePlayer(Transform target)
    {
        if (IsWallDetected() || (ledgeCheck != null && ledgeCheck.IsDetectingLedge()))
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
        anim.SetTrigger(animStun); 
        Debug.Log("Nấm tự gây choáng bản thân!");
    }
}