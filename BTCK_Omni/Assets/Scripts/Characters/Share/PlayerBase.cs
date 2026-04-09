        using System;
        using System.Collections;
        using System.Security.Cryptography;
        using UnityEngine;

        public class PlayerBase : Entity
        {
            [Header("Player Settings")] 
            [SerializeField] public int playerIndex;

            [Header("Keybindings")] 
            [SerializeField] protected KeyCode keyLeft;
            [SerializeField] protected KeyCode keyRight;
            [SerializeField] protected KeyCode keyJump;
            [SerializeField] protected KeyCode keyAttack;
            [SerializeField] protected KeyCode keyDef;
            [SerializeField] protected KeyCode keySpAtk;
            [SerializeField] protected KeyCode keyRoll;
            
            
            [Header("Jump Settings")]
            [SerializeField] private float jumpForce;
            [SerializeField] private int maxJumps;
            
            [Header("Roll Settings")]
            [SerializeField] private float rollForce;
            [SerializeField] private float rollDuration;
            

            [Header("Groundcheck settings")]
            [SerializeField] private GroundChecker _groundChecker;
         
            [Header("Attack settings")]
            //[SerializeField] private float attackCooldown;
            
            //velocity
            private Vector2 targetVelocity;
            private bool useTargetVelocity = false;

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
            
            
            //Update
            protected void Update()
            {
                if (!inputEnabled || isDead) return;
                
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
            protected virtual void OnUpdate(){}
            
            //FixedUpdate
            private void FixedUpdate()
            {
                
                _groundChecker.Check(jumpDisableTimer > 0);
                wasGrounded = isGrounded;
                isGrounded = _groundChecker.IsGrounded;
                isOnSlope = _groundChecker.IsOnSlope;
                if (isGrounded && !wasGrounded)
                {
                    anim.ResetTrigger("Attack");
                    anim.ResetTrigger("AirAttack");
                    anim.ResetTrigger("Jump");
                }
                
                if (useTargetVelocity)
                {
                    rb.velocity = targetVelocity;
                    useTargetVelocity = false;
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
                    HandleMovement();
                    ApplyJump();
                    
                }
                if (isOnSlope && isGrounded && !jumpRequested && Mathf.Abs(rb.velocity.x) < 0.1f && rb.velocity.y <= 0.1f)
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
                if (isAttacking || isRolling || isDefending)
                {
                    useTargetVelocity = false;
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
                    Vector2 moveVelocity = moveSpeed * -dir * slopeDir ;
                    rb.velocity = moveVelocity;
                }
                else
                {
                    SetVelocityX(dir * moveSpeed);
                }
                if(wasGrounded && !jumpRequested && !isGrounded) SetVelocityY(0);
            }
            #endregion

            
            
            #region Jump
            private void HandleJumpInput()
            {
                if (isDefending || isRolling) return;
                if (Input.GetKeyDown(keyJump) && !isAttacking  )
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
                    useTargetVelocity = false;
                    
                    SetVelocityY(0); 
                    SetVelocityY(jumpForce);
                    anim.SetTrigger("Jump");
                    
                    isGrounded = false;
                    jumpRequested = false;
                    jumpCount++;
                    hasAirAttack = false;
                }
            }
            
            
            #endregion


            #region Attack
            private void HandleAttackInput()
            {
                if (isDefending || isRolling) return;
                if (Input.GetKeyDown(keyAttack) && !isAttacking)
                {
                    if(isGrounded) Attack(false);
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
                anim.SetTrigger("Roll");
                rollDir = facingDir;
                activeRollForce = force;
                yield return new WaitForSeconds(rollDuration);
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
                    anim.speed =0;
                }
            }
            
            #endregion
            
            
            private void UpdateAnimation()
            {
                anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
                anim.SetBool("IsGrounded", isGrounded);
                anim.SetFloat("VelocityY", rb.velocity.y);
                anim.SetBool("IsDefending", isDefending);
            }
            
            
            #region TakeDamage and Die

            public override void TakeDamage(float dmg, Vector2 hitDir)
            {
                if (isDead) return;
                if (isRolling) return; 
                
                if (isDefending)
                {
                    dmg *= 0.1f; 
                    targetVelocity = hitDir.normalized * (knockbackForce * 0.5f);
                    useTargetVelocity = true;
                    
                    currentHP = Mathf.Max(0, currentHP - dmg);
                    OnHPChanged?.Invoke(currentHP, maxHP);
                    if(currentHP <= 0) Die();
                    return;
                }
                isAttacking = false;
                StopAllCoroutines();
                anim.speed = 1f; 
                base.TakeDamage(dmg, hitDir);
                targetVelocity = hitDir.normalized * knockbackForce;
                useTargetVelocity = true;
            }
            
            protected override void Die()
            {
                StopAllCoroutines();
                isAttacking = false;
                inputEnabled = false;
                base.Die();
            }
            #endregion

            
        }