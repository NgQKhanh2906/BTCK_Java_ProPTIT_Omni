using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerBase : Entity, IHealable, ISaveable, IInteractable
{
    [Header("Player Settings")] [SerializeField]
    public int playerIndex = 1;

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

    [Header("Respawn Settings")] [SerializeField]
    protected float HealthAfterRespawn;

    [SerializeField] protected float ManaAfterRespawn;
    [SerializeField] protected float invincibilityTime = 5f;
    [SerializeField] private GameObject environmentalDeathVFXPrefab;

    [Header("Jump Settings")] [SerializeField]
    private float jumpForce;

    [SerializeField] protected int maxJumps;

    [Header("Roll Settings")] [SerializeField]
    private float rollForce;

    [SerializeField] private float rollDuration;
    private WaitForSeconds rollWait;

    [Header("Groundcheck settings")] [SerializeField]
    private GroundChecker _groundChecker;

    [SerializeField] protected Transform headPlatform;
    [SerializeField] protected Vector2 headSize;

    [Header("Mana Settings")] [SerializeField]
    protected float maxMana = 100f;

    [SerializeField] protected float manaPerHit = 15f;
    [SerializeField] protected float rollManaCost = 20f;
    [SerializeField] protected float manaRegen = 5f;

    [Header("SFX")] [SerializeField] protected SfxManager sfx;
    [SerializeField] private float stepInterval = 0.35f;

    private float stepTimer = 0f;

    // ui
    private float manaUiTimer = 0f;
    protected float currentMana;
    public float CurrentMana => currentMana;
    public float MaxMana => maxMana;
    public event Action<float, float> OnManaChanged;

    // respawn
    public event Action OnRespawnEvent;
    protected Vector3 lastSafePos;
    public Vector3 LastSafePos => lastSafePos;

    //knockedback
    private Coroutine knockbackCoroutine;
    private bool isKnockedBack;

    //layer
    private int pLayer;
    [SerializeField] protected LayerMask enemyLayerMask;
    private Collider2D col2d;


    //groundcheck
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

    //jump
    private float jumpDisableTimer;
    private bool jumpRequested;
    private int jumpCount;
    private bool hasAirAttack;

    //def
    protected bool isDefending;

    //input
    private bool inputEnabled = true;

    //interact
    public bool isCorpseLost;
    public bool CanInteract => isDead && !isCorpseLost;

    protected override void Awake()
    {
        base.Awake();
        currentMana = maxMana;
        rollWait = new WaitForSeconds(rollDuration);
        pLayer = LayerMask.NameToLayer("Player");
        if (enemyLayerMask.value == 0)
        {
            enemyLayerMask = LayerMask.GetMask("Enemy", "FlyingEnemy", "Boss");
        }

        col2d = GetComponent<Collider2D>();
        HealthAfterRespawn = maxHP * 0.3f;
        ManaAfterRespawn = maxMana * 0.5f;
    }

    protected virtual void Start()
    {
        inputEnabled = true;
        isDead = false;
        isAttacking = false;
        isDefending = false;
        isRolling = false;
        isKnockedBack = false;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector2.zero;
        }

        if (anim != null) anim.speed = 1f;
        if (sr != null) sr.enabled = true;
        if (col2d != null) col2d.enabled = true;
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
        UpdateAnimation();
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
    }

    private void FixedUpdate()
    {
        if (!inputEnabled || isDead) return;
        _groundChecker.Check(jumpDisableTimer > 0);
        wasGrounded = isGrounded;
        isGrounded = _groundChecker.IsGrounded;
        isOnSlope = _groundChecker.IsOnSlope;

        if (isGrounded && !isRolling && !isAttacking)
        {
            lastSafePos = transform.position;
        }

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
            else
            {
                SetVelocityX(rollDir * activeRollForce);
            }
        }
        else
        {
            if (!isKnockedBack)
            {
                HandleMovement();
                ApplyJump();
            }
        }

        if (!isKnockedBack && isOnSlope && isGrounded && !jumpRequested && Mathf.Abs(rb.velocity.x) < 0.1f &&
            rb.velocity.y <= 0.1f)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
        else
        {
            rb.gravityScale = 3f;
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
        if (inputEnabled)
        {
            if (Input.GetKey(keyLeft)) dir = -1f;
            if (Input.GetKey(keyRight)) dir = 1f;
        }

        if (dir != 0 && dir != facingDir)
        {
            Flip();
        }

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

        if (wasGrounded && !jumpRequested && !isGrounded)
        {
            SetVelocityY(0);
        }

        if (isGrounded && Mathf.Abs(rb.velocity.x) > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                if (sfx != null)
                {
                    sfx.PlayWalk();
                }

                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
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

            if (sfx != null)
            {
                sfx.PlayJump();
            }

            isGrounded = false;
            jumpRequested = false;
            jumpCount++;
            hasAirAttack = false;
        }
    }

    private void SetIgnorePlayerVsEnemyLayers(bool ignore)
    {
        int m = enemyLayerMask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((m & (1 << i)) != 0)
            {
                Physics2D.IgnoreLayerCollision(pLayer, i, ignore);
            }
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
            if (!isOverlapping)
            {
                SetIgnorePlayerVsEnemyLayers(false);
            }
        }
    }

    #endregion


    #region Attack

    private void HandleAttackInput()
    {
        if (isDefending || isRolling) return;
        if (isKnockedBack) return;
        if (Input.GetKeyDown(keyAttack) && !isAttacking)
        {
            jumpRequested = false;
            if (isGrounded)
            {
                Attack(false);
            }
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

            if (sfx != null)
            {
                sfx.PlayRoll();
            }

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

    #region Take Damage

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
            StartKnockback();
            if (currentHP <= 0)
            {
                Die();
            }

            return;
        }

        isAttacking = false;
        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);
            rollCoroutine = null;
        }

        anim.speed = 1f;
        base.TakeDamage(dmg, hitDir);

        if (sfx != null)
        {
            sfx.PlayHurt();
        }

        StartKnockback();
    }

    private void StartKnockback()
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(ApplyKnockbackLock());
    }

    private IEnumerator ApplyKnockbackLock()
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(0.5f);
        isKnockedBack = false;
    }

    #endregion


    #region Save

    [Serializable]
    public class PlayerSaveState
    {
        public float hp, mana, posX, posY;
        public float safeX, safeY;
        public bool isDead;
        public bool isCorpseLost;
    }

    public object CaptureState() => new PlayerSaveState
    {
        hp = currentHP,
        mana = currentMana,
        posX = transform.position.x,
        posY = transform.position.y,
        safeX = lastSafePos.x,
        safeY = lastSafePos.y,
        isDead = isDead,
        isCorpseLost = isCorpseLost
    };

    public void RestoreState(object state)
    {
        var s = (PlayerSaveState)state;
        currentHP = s.hp;
        NotifyHPChanged();
        currentMana = s.mana;
        OnManaChanged?.Invoke(currentMana, maxMana);
        transform.position = new Vector3(s.posX, s.posY, 0);
        lastSafePos = new Vector3(s.safeX, s.safeY, 0);

        if (s.isDead)
        {
            ForceDeadState(s.isCorpseLost);
        }
    }

    #endregion


    #region Death

    public override void Die()
    {
        StopAllCoroutines();
        isAttacking = false;
        inputEnabled = false;
        currentMana = 0;
        OnManaChanged?.Invoke(currentMana, maxMana);
        if (sfx != null) sfx.PlayDie();
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        if (col2d != null) col2d.isTrigger = true;
        if (headPlatform != null)
        {
            Collider2D hc = headPlatform.GetComponent<Collider2D>();
            if (hc != null) hc.enabled = false;
        }

        base.Die();
    }

    public void BecomeInteractableCorpse()
    {
        if (!isCorpseLost)
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
    }

    public void DieFromEnvironment()
    {
        if (isDead) return;
        isDead = true;
        isCorpseLost = true;
        inputEnabled = false;
        isAttacking = false;
        StopAllCoroutines();
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        if (environmentalDeathVFXPrefab != null)
        {
            GameObject vfx = Instantiate(environmentalDeathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 0.6f);
        }

        sr.enabled = false;
        if (col2d != null)
        {
            col2d.enabled = false;
        }

        if (headPlatform != null)
        {
            Collider2D hc = headPlatform.GetComponent<Collider2D>();
            if (hc != null) hc.enabled = false;
        }

        currentHP = 0;
        currentMana = 0;
        NotifyHPChanged();
        OnManaChanged.Invoke(currentMana, maxMana);
        if (sfx != null)
        {
            sfx.PlayDie();
        }

        NotifyDeath();
    }

    #endregion


    #region Respawn

    public void Respawn(Vector3 position)
    {
        isDead = false;
        isAttacking = false;
        isDefending = false;
        isRolling = false;
        isKnockedBack = false;
        inputEnabled = true;
        isCorpseLost = false;
        anim.speed = 1f;
        gameObject.layer = LayerMask.NameToLayer("Player");
        transform.position = position;
        rb.velocity = Vector2.zero;
        rb.isKinematic = false;
        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);
            rollCoroutine = null;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }

        currentHP = HealthAfterRespawn;
        NotifyHPChanged();
        currentMana = ManaAfterRespawn;
        OnManaChanged?.Invoke(currentMana, maxMana);
        rb.isKinematic = false;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        sr.enabled = true;

        if (col2d)
        {
            col2d.enabled = true;
            col2d.isTrigger = false;
        }

        if (headPlatform)
        {
            Collider2D hc = headPlatform.GetComponent<Collider2D>();
            if (hc) hc.enabled = true;
        }

        anim.SetTrigger(GameConfig.ANIM_COL_RESPAWN);
        StartCoroutine(InvincibleRoutine(invincibilityTime));
        OnRespawnEvent?.Invoke();
    }

    private IEnumerator InvincibleRoutine(float time)
    {
        isInvincible = true;
        if (sr != null)
        {
            sr.DOFade(0.2f, 0.15f).SetLoops(-1, LoopType.Yoyo).SetId("InvincibilityBlink").SetLink(gameObject);
        }

        yield return new WaitForSeconds(time);
        isInvincible = false;
        DOTween.Kill("InvincibilityBlink");
        if (sr != null)
        {
            sr.DOFade(1f, 0.1f).SetLink(gameObject);
        }
    }

    #endregion


    #region InteractAfterDie

    public void Interact(PlayerBase p)
    {
        if (LivesManager.Instance != null)
        {
            int l = LivesManager.Instance.GetLives(p.playerIndex);
            if (l >= 2)
            {
                LivesManager.Instance.SetLivesDirectly(p.playerIndex, l - 1);
                LivesManager.Instance.SetLivesDirectly(playerIndex, 1);
                isCorpseLost = false;
                LivesManager.Instance.TriggerRespawn(this, transform.position);
            }
        }
    }

    public void ForceDeadState(bool lost)
    {
        isDead = true;
        isCorpseLost = lost;
        inputEnabled = false;
        isAttacking = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        if (col2d != null) col2d.isTrigger = true;
        if (headPlatform != null)
        {
            Collider2D hc = headPlatform.GetComponent<Collider2D>();
            if (hc != null) hc.enabled = false;
        }

        if (lost)
        {
            if (sr != null) sr.enabled = false;
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            if (sr != null) sr.enabled = true;
            anim.Play(GameConfig.ANIMATOR_DIE, 0, 1f);
            anim.speed = 0f;
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
    }

    #endregion

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (headPlatform != null)
        {
            Gizmos.DrawWireCube(headPlatform.position, headSize);
        }
    }
}