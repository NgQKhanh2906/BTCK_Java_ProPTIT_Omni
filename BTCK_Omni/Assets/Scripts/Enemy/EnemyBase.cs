using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(PlayerDetector))]
[RequireComponent(typeof(AudioSource))]
public class EnemyBase : Entity
{
    [Header("Animator Parameters")]
    protected readonly int animIsMoving = Animator.StringToHash("isMoving");
    protected readonly int animStun = Animator.StringToHash("Stun");
    protected readonly int animIsHiding = Animator.StringToHash("isHiding");

    [Header("--- SOUND EFFECTS (SFX) ---")] 
    [SerializeField] protected AudioClip hitSound;
    [SerializeField] protected AudioClip attackSound;
    [Range(0f, 1f)] [SerializeField] protected float sfxVolume = 0.8f;
    
    protected AudioSource audioSource; 


    [Header("Unity Events")]
    public UnityEvent onEnemyDeath;

    [Header("Hit Flash VFX")]
    [SerializeField] private Material whiteFlashMat; 
    [SerializeField] private float flashDuration = 0.15f; 
    private Material originalMat; 
    private Coroutine flashRoutine;

    [Header("Home & Tethering")]
    [SerializeField] protected float maxWanderDistance = 6f;
    protected Vector2 startPosition;
    protected bool isReturningHome = false;
    protected float patrolBoundLeft;
    protected float patrolBoundRight;

    [Header("Collision Checks")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected Vector2 wallCheckSize = new Vector2(0.1f, 0.8f);
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] protected LedgeDetector ledgeCheck;

    [Header("Idle Settings")]
    [SerializeField] protected float idleDuration = 2f;
    protected float idleTimer;
    protected bool isIdle;
    protected PlayerDetector playerDetector;


    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
        playerDetector = GetComponent<PlayerDetector>();
        this.OnDeath += ForwardDeathEvent;
        float targetB = startPosition.x + (maxWanderDistance * facingDir);
        patrolBoundLeft = Mathf.Min(startPosition.x, targetB);
        patrolBoundRight = Mathf.Max(startPosition.x, targetB);
        if (sr != null) originalMat = sr.material;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }


    protected virtual void OnDestroy()
    {
        this.OnDeath -= ForwardDeathEvent;
    }


    private void ForwardDeathEvent()
    {
        onEnemyDeath?.Invoke();
    }
    protected virtual Transform GetVisiblePlayer()
    {
        if (playerDetector != null && playerDetector.CanSeePlayer())
        {
            return playerDetector.GetPlayerTransform();
        }
        return null;
    }

    public virtual void PlayHitSFX()
    {
        if (hitSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); 
            audioSource.PlayOneShot(hitSound, sfxVolume);
        }
    }

    public virtual void PlayAttackSFX()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(attackSound, sfxVolume);
        }
    }


    protected virtual void Update()
    {
        if (isDead) return;


        if (isReturningHome)
        {
            ReturnHomeLogic();
            return;
        }


        if (isIdle) HandleIdle();
        else HandlePatrol();
    }


    protected virtual void ReturnHomeLogic()
    {
        float distanceToHome = startPosition.x - transform.position.x;


        if (Mathf.Abs(distanceToHome) < 0.5f)
        {
            isReturningHome = false;
            StartIdle();
            return;
        }


        bool isLedgeAhead = ledgeCheck != null && ledgeCheck.IsDetectingLedge();
        bool isWallAhead = IsWallDetected();


        if (isLedgeAhead || isWallAhead)
        {
            startPosition = transform.position;
            isReturningHome = false;
            StartIdle();
            return;
        }


        int moveDir = distanceToHome > 0 ? 1 : -1;
        if (moveDir != facingDir) Flip();
       
        SetVelocityX(moveSpeed * facingDir);
        UpdateAnimation(true);
        isIdle = false;
    }


    protected virtual void HandlePatrol()
    {
        bool isLedgeAhead = ledgeCheck != null && ledgeCheck.IsDetectingLedge();
        bool isWallAhead = IsWallDetected();
        bool reachedLeft = transform.position.x <= patrolBoundLeft && facingDir == -1;
        bool reachedRight = transform.position.x >= patrolBoundRight && facingDir == 1;
        if (isLedgeAhead || isWallAhead || reachedLeft || reachedRight)
        {
            StartIdle();
        }
        else
        {
            SetVelocityX(moveSpeed * facingDir);
            UpdateAnimation(true);
        }
    }

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        base.TakeDamage(dmg, hitDir);
        if (sr != null && whiteFlashMat != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine); 
            flashRoutine = StartCoroutine(FlashCoroutine());
        }
        PlayHitSFX();
    }
    private System.Collections.IEnumerator FlashCoroutine()
    {
        sr.material = whiteFlashMat; 
        yield return new WaitForSeconds(flashDuration); 
        sr.material = originalMat; 
    }

    public override void Die()
    {
        base.Die();
        Destroy(gameObject); 
    }

    protected virtual void StartIdle()
    {
        isIdle = true;
        idleTimer = idleDuration;
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


    protected virtual void ChasePlayer(Transform target)
    {
        isIdle = false;
        UpdateAnimation(true);
    }


    protected virtual void UpdateAnimation(bool isMoving)
    {
        if (anim != null) anim.SetBool(animIsMoving, isMoving);
    }


    public void SetVelocityX(float velocityX)
    {
        if (rb != null) rb.velocity = new Vector2(velocityX, rb.velocity.y);
    }


    protected bool IsWallDetected()
    {
        if (wallCheck == null) return false;
        return Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0, new Vector2(facingDir, 0), 0.1f, whatIsGround);
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
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        if (Application.isPlaying)
        {
            float width = patrolBoundRight - patrolBoundLeft;
            float centerX = patrolBoundLeft + width / 2f;
            Gizmos.DrawWireCube(new Vector3(centerX, startPosition.y, 0), new Vector3(width, 1, 0));
        }
        else
        {
            float targetB = transform.position.x + (maxWanderDistance * facingDir);
            float minX = Mathf.Min(transform.position.x, targetB);
            float maxX = Mathf.Max(transform.position.x, targetB);
            float width = maxX - minX;
            float centerX = minX + width / 2f;
            Gizmos.DrawWireCube(new Vector3(centerX, transform.position.y, 0), new Vector3(width, 1, 0));
        }
    }
}

