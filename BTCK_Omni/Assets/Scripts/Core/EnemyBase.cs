using UnityEngine;

public class EnemyBase : Entity
{
    [Header("Patrol Settings")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float groundCheckDistance = 1f;
    [SerializeField] protected float wallCheckDistance = 0.5f;
    [SerializeField] protected LayerMask whatIsGround;

    [Header("Patrol Timings")]
    [SerializeField] protected float patrolDuration = 2f;
    protected float patrolTimer;

    [Header("Idle Settings")]
    [SerializeField] protected float idleDuration = 2f;
    protected float idleTimer;
    protected bool isAdle;
    protected override void Awake()
    {
        base.Awake();
        patrolTimer = patrolDuration;
    }
    protected virtual void Update()
    {
        if (isDead) return;

        if (isAdle)
        {
            HandleIdle();
        }
        else
        {
            HandlePatrol();
        }
    }

    protected virtual void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;
        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        bool isWallAhead = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        if (patrolTimer <= 0 || !isGroundAhead || isWallAhead)
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
        isAdle = true;
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
            isAdle = false;
            Flip();
        }
    }

    protected virtual void UpdateAnimation(bool isMoving)
    {
        if (anim != null)
            anim.SetBool("isMoving", isMoving);
    }
}