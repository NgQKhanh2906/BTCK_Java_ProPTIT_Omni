using UnityEngine;

public class EnemyBase : Entity
{
    [Header("Animator Parameters")]
    protected readonly int animIsMoving = Animator.StringToHash("isMoving");
    protected readonly int animStun = Animator.StringToHash("Stun");
    protected readonly int animIsHiding = Animator.StringToHash("isHiding");

    [Header("Collision Checks (BoxCast)")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); 
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected Vector2 wallCheckSize = new Vector2(0.1f, 0.8f);
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] protected LedgeDetector ledgeCheck;

    [Header("Patrol Timings")]
    [SerializeField] protected float patrolDuration = 2f;
    protected float patrolTimer;

    [Header("Idle Settings")]
    [SerializeField] protected float idleDuration = 2f;
    protected float idleTimer;
    protected bool isIdle; 

    protected override void Awake()
    {
        base.Awake();
        patrolTimer = patrolDuration;
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (isIdle)
        {
            HandleIdle();
        }
        else
        {
            HandlePatrol();
        }
    }

    public virtual bool IsGrounded()
    {
        return Physics2D.BoxCast(groundCheck.position, groundCheckSize, 0f, Vector2.down, 0.1f, whatIsGround);
    }

    public virtual bool IsWallDetected()
    {
        return Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0f, new Vector2(facingDir, 0), 0.1f, whatIsGround);
    }

 protected virtual void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;
        bool isLedgeAhead = ledgeCheck != null && ledgeCheck.IsDetectingLedge();
        bool isWallAhead = IsWallDetected();
        if (patrolTimer <= 0 || isLedgeAhead || isWallAhead)
        {
            StartIdle();
        }
        else
        {
            SetVelocityX(moveSpeed * facingDir);
            UpdateAnimation(true);
        }
    }
    protected virtual void StartIdle()
    {
        isIdle = true;
        idleTimer = idleDuration;
        patrolTimer = patrolDuration;
        SetVelocityX(0);
        UpdateAnimation(false);
    }
    protected virtual void HandleIdle()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0)
        {
            isIdle = false;
            Flip();
        }
    }
    protected virtual void UpdateAnimation(bool isMoving)
    {
        if (anim != null)
        {
            anim.SetBool(animIsMoving, isMoving);
        }
    }
    protected virtual void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position - new Vector3(0, 0.1f, 0), groundCheckSize);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wallCheck.position + new Vector3(0.1f * facingDir, 0, 0), wallCheckSize);
        }
    }
}