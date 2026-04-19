using System;
using System.Collections;
//using System.Security.Cryptography;
using UnityEngine;

public class PlayerBase : Entity, IHealable, ISaveable
{
    [Header("Player Settings")] [SerializeField]
    public int playerIndex;

    [SerializeField] private string uniqueId = Guid.NewGuid().ToString();
    public string UniqueId => uniqueId;

    [Header("Keybindings")] [SerializeField]
    protected KeyCode keyLeft;

    [SerializeField] protected KeyCode keyRight;
    [SerializeField] protected KeyCode keyJump;
    [SerializeField] protected KeyCode keyAttack;
    [SerializeField] protected KeyCode keyDef;
    [SerializeField] protected KeyCode keySpAtk;
    [SerializeField] protected KeyCode keyRoll;
    [SerializeField] protected KeyCode keyInteract;


    [Header("Jump Settings")] [SerializeField]
    private float jumpForce;

    [SerializeField] private int maxJumps;

    [Header("Roll Settings")] [SerializeField]
    private float rollForce;

    [SerializeField] private float rollDuration;
    private WaitForSeconds rollWait;


    [Header("Groundcheck settings")] [SerializeField]
    private GroundChecker _groundChecker;

    [Header("Mana Settings")] [SerializeField]
    protected float maxMana = 100f;

    [SerializeField] protected float manaPerHit = 15f;
    [SerializeField] protected float rollManaCost = 20f;
    [SerializeField] protected float manaRegen = 5f;


    //mana
    private float manaUiTimer = 0f;
    protected float currentMana;
    public float CurrentMana => currentMana;
    public float MaxMana => maxMana;
    public event Action<float, float> OnManaChanged;

    //[Header("Attack settings")]
    //[SerializeField] private float attackCooldown;

    //knockedback
    private bool isKnockedBack;
    
    //id
    private int pLayer;
    [Tooltip("Các layer coi là địch (đánh, pass-through). Có thể gán trong Inspector; nếu để trống sẽ dùng Enemy + FlyingEnemy + Boss.")]
    [SerializeField] protected LayerMask enemyLayerMask;
    private Collider2D col2d;

    //ground
    protected bool isGrounded;
    protected bool isOnSlope;
    protected bool wasGrounded;

    //roll
    protected bool isRolling;
    protected float rollDir;
    private float activeRollForce;
    private Coroutine rollCoroutine;

    //attack
    protected bool isAttacking;
    //private float attackTimer;

    //jump
    private float jumpDisableTimer;
    private bool jumpRequested;
    private int jumpCount;
    private bool hasAirAttack;

    //defend
    protected bool isDefending;

    //key
    private bool inputEnabled = true;

    // interact
    private IInteractable _nearbyInteractable;

    protected override void Awake()
    {
        base.Awake();
        currentMana = maxMana;
        rollWait = new WaitForSeconds(rollDuration);
        pLayer = LayerMask.NameToLayer("Player");
        if (enemyLayerMask.value == 0)
            enemyLayerMask = LayerMask.GetMask("Enemy", "FlyingEnemy", "Boss");
        col2d = GetComponent<Collider2D>();
    }

    //Update
    protected void Update()
    {
        if (!inputEnabled || isDead) return;
        HandlePassiveMana();
        HandlePassThrough();
        if (jumpDisableTimer > 0)
        {
            jumpDisableTimer -= Time.deltaTime;
        }

        if (isGrounded && jumpDisableTimer <= 0)
        {
            jumpCount = 0;
            hasAirAttack = false;
        }

        HandleJumpInput();
        HandleAttackInput();
        HandleRollInput();
        HandleDefend();
        HandleInteract();
        UpdateAnimation();
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
    }


    //FixedUpdate
    private void FixedUpdate()
    {
        if (isDead) return;
        _groundChecker.Check(jumpDisableTimer > 0);
        wasGrounded = isGrounded;
        isGrounded = _groundChecker.IsGrounded;
        isOnSlope = _groundChecker.IsOnSlope;
        if (isGrounded && !wasGrounded)
        {
            anim.ResetTrigger(GameConfig.ANIM_COL_ATTACK);
            anim.ResetTrigger(GameConfig.ANIM_COL_AIRATTACK);
            anim.ResetTrigger(GameConfig.ANIM_COL_JUMP);
        }

        if (isRolling)
        {
            if (isOnSlope)
            {
                CancleRoll();
            }
            else SetVelocityX(rollDir * activeRollForce);
        }

        else
        {
            if (!isKnockedBack)
            {
                HandleMovement();
                ApplyJump();
            }
        }

        if (!isKnockedBack && isOnSlope && isGrounded && !jumpRequested && Mathf.Abs(rb.velocity.x) < 0.1f && rb.velocity.y <= 0.1f)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }


    #region Movement

    private void HandleMovement()
    {
        if (isAttacking || isRolling || isDefending || isKnockedBack)
        {
            return;
        }

        float dir = 0f;
        if (Input.GetKey(keyLeft)) dir = -1f;
        if (Input.GetKey(keyRight)) dir = 1f;

        if (dir != 0 && dir != facingDir)
            Flip();

        if (isOnSlope && isGrounded && !jumpRequested)
        {
            Vector2 slopeDir = Vector2.Perpendicular(_groundChecker.SlopeNormal).normalized;
            Vector2 moveVelocity = moveSpeed * -dir * slopeDir;
            rb.velocity = moveVelocity;
        }
        else
        {
            SetVelocityX(dir * moveSpeed);
        }

        if (wasGrounded && !jumpRequested && !isGrounded) SetVelocityY(0);
    }

    #endregion


    #region Jump

    private void HandleJumpInput()
    {
        if (isDefending || isRolling) return;
        if (Input.GetKeyDown(keyJump) && !isAttacking)
        {
            if (isGrounded || jumpCount < maxJumps)
            {
                jumpRequested = true;
            }
        }
    }

    private void ApplyJump()
    {
        if (jumpRequested)
        {
            jumpDisableTimer = 0.15f;
            rb.gravityScale = 1f;

            SetVelocityY(0);
            SetVelocityY(jumpForce);
            anim.SetTrigger(GameConfig.ANIM_COL_JUMP);

            isGrounded = false;
            jumpRequested = false;
            jumpCount++;
            hasAirAttack = false;
        }
    }

    #endregion


    /// <summary>Bật/tắt bỏ qua va chạm Player với mọi layer trong enemyLayerMask (mỗi cặp layer chỉ gọi IgnoreLayerCollision một lần).</summary>
    private void SetIgnorePlayerVsEnemyLayers(bool ignore)
    {
        int m = enemyLayerMask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((m & (1 << i)) != 0)
                Physics2D.IgnoreLayerCollision(pLayer, i, ignore);
        }
    }

    private void HandlePassThrough()
    {
        if (!isGrounded || isRolling)
        {
            SetIgnorePlayerVsEnemyLayers(true);
        }
        else
        {
            bool isOverlapping = Physics2D.OverlapBox(col2d.bounds.center, col2d.bounds.size, 0f, enemyLayerMask);
            if (!isOverlapping) SetIgnorePlayerVsEnemyLayers(false);
        }
    }


    #region Attack

    private void HandleAttackInput()
    {
        if (isDefending || isRolling) return;
        if (Input.GetKeyDown(keyAttack) && !isAttacking)
        {
            jumpRequested = false;
            if (isGrounded) Attack(false);
            else if (!hasAirAttack)
            {
                hasAirAttack = true;
                Attack(true);
            }
        }
    }

    protected virtual void Attack(bool hasUsedAirAttack)
    {
    }

    #endregion


    #region Roll

    private void HandleRollInput()
    {
        if (isAttacking || !isGrounded || isOnSlope || isRolling || isDefending) return;
        if (Input.GetKeyDown(keyRoll))
        {
            if (currentMana < rollManaCost) return;
            currentMana -= rollManaCost;
            OnManaChanged?.Invoke(currentMana, maxMana);
            float curSpeed = Mathf.Abs(rb.velocity.x);
            float finalRollForce = rollForce;
            if (curSpeed > 0.1f)
            {
                finalRollForce += (0.5f * curSpeed);
            }

            rollCoroutine = StartCoroutine(ApplyRoll(finalRollForce));
        }
    }

    private IEnumerator ApplyRoll(float force)
    {
        isRolling = true;
        anim.SetTrigger(GameConfig.ANIM_COL_ROLL);
        rollDir = facingDir;
        activeRollForce = force;
        yield return rollWait;
        isRolling = false;
        rollCoroutine = null;
    }

    private void CancleRoll()
    {
        if (isRolling)
        {
            isRolling = false;
            if (rollCoroutine != null)
            {
                StopCoroutine(rollCoroutine);
                rollCoroutine = null;
            }
        }
    }

    #endregion


    #region Defend

    private void HandleDefend()
    {
        if (isAttacking || isRolling || !isGrounded) return;
        if (Input.GetKey(keyDef))
        {
            jumpRequested = false;
            isDefending = true;
            SetVelocityX(0);
        }
        else if (Input.GetKeyUp(keyDef))
        {
            isDefending = false;
            anim.speed = 1;
        }
    }

    private void HoldDef()
    {
        if (isDefending)
        {
            anim.speed = 0;
        }
    }

    #endregion

    #region Interactable

    private void HandleInteract()
    {
        if (_nearbyInteractable == null) return;
        if (Input.GetKeyDown(keyInteract) && _nearbyInteractable.CanInteract)
            _nearbyInteractable.Interact(this);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var i = col.GetComponent<IInteractable>();
        if (i != null) _nearbyInteractable = i;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        var i = col.GetComponent<IInteractable>();
        if (i != null && _nearbyInteractable == i) _nearbyInteractable = null;
    }

    #endregion

    #region Heal

    public void RestoreHP(float amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        NotifyHPChanged();
    }

    public void RestoreMana(float amount)
    {
        currentMana = Mathf.Min(maxMana, currentMana + amount);
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    private void HandlePassiveMana()
    {
        if (isAttacking || isRolling || isDefending) return;
        if (currentMana < maxMana)
        {
            currentMana += manaRegen * Time.deltaTime;
            bool isFull = currentMana >= maxMana;
            if (isFull) currentMana = maxMana;
            manaUiTimer += Time.deltaTime;
            if (manaUiTimer >= 1f || isFull)
            {
                OnManaChanged?.Invoke(currentMana, maxMana);
                manaUiTimer = 0f;
            }
        }
    }

    #endregion

    private void UpdateAnimation()
    {
        anim.SetFloat(GameConfig.ANIM_COL_SPEED, Mathf.Abs(rb.velocity.x));
        anim.SetBool(GameConfig.ANIM_COL_IS_GROUNDED, isGrounded);
        anim.SetFloat(GameConfig.ANIM_COL_VERLOCITYY, rb.velocity.y);
        anim.SetBool(GameConfig.ANIM_COL_IS_DEFENDING, isDefending);
    }


    #region TakeDamage and Die

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        if (isDead || isRolling) return;
        if (isDefending)
        {
            dmg *= 0.1f;
            currentHP = Mathf.Max(0, currentHP - dmg);
            NotifyHPChanged();
            rb.velocity = Vector2.zero;
            rb.AddForce(hitDir.normalized * knockbackForce, ForceMode2D.Impulse);
            StartCoroutine(ApplyKnockbackLock());
            if (currentHP <= 0)
                Die();
            return;
        }

        isAttacking = false;
        StopAllCoroutines();
        anim.speed = 1f;
        base.TakeDamage(dmg, hitDir);
        StartCoroutine(ApplyKnockbackLock());
    }
    private IEnumerator ApplyKnockbackLock()
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(0.5f);
        isKnockedBack = false;
    }
    
    [Serializable]
    public class PlayerSaveState
    {
        public float hp, mana, posX, posY;
    }

    public object CaptureState() => new PlayerSaveState
    {
        hp = currentHP, mana = currentMana,
        posX = transform.position.x, posY = transform.position.y
    };

    public void RestoreState(object state)
    {
        var s = (PlayerSaveState)state;
        currentHP = s.hp;
        NotifyHPChanged();
        currentMana = s.mana;
        OnManaChanged?.Invoke(currentMana, maxMana);
        transform.position = new Vector3(s.posX, s.posY, 0);
    }

    public override void Die()
    {
        StopAllCoroutines();
        isAttacking = false;
        inputEnabled = false;
        base.Die();
    }

    #endregion
}