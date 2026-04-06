    using System;
    using System.Collections;
    using System.Security.Cryptography;
    using UnityEngine;

    public class PlayerBase : Entity
    {
        [Header("Player Settings")] 
        [SerializeField] public int playerIndex = 0;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float attackCooldown = 0.5f;

        [SerializeField] private GroundChecker _groundChecker;
     
        
        private Vector2 targetVelocity;
        private bool useTargetVelocity = false;

        protected bool isJumping = false;
        protected bool isGrounded;
        protected bool isOnSlope;
        protected bool wasGrounded;
        protected bool isAttacking;
        private bool inputEnabled = true;
        private bool jumpRequested;
        private float attackTimer;
        private bool lockVelocityX;
        private KeyCode keyLeft, keyRight, keyJump, keyAttack;

        protected override void Awake()
        {
            base.Awake();
            SetupInput();
        }

        private void SetupInput()
        {
            if (playerIndex == 0)
            {
                keyLeft = KeyCode.A;
                keyRight = KeyCode.D;
                keyJump = KeyCode.W;
                keyAttack = KeyCode.J;
            }
            else
            {
                keyLeft = KeyCode.LeftArrow;
                keyRight = KeyCode.RightArrow;
                keyJump = KeyCode.UpArrow;
                keyAttack = KeyCode.Alpha1;
            }
        }

        protected virtual void Update()
        {
            if (!inputEnabled || isDead) return;

            attackTimer -= Time.deltaTime;
            HandleJumpInput();
            HandleAttackInput();
            anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            anim.SetBool("IsGrounded", isGrounded);
            anim.SetFloat("VelocityY", rb.velocity.y);
            OnUpdate();
        }
        
        private void FixedUpdate()
        {
            if (rb.velocity.y <= 0.01f)
            {
                isJumping = false;
            }
            _groundChecker.Check(isJumping);
            wasGrounded = isGrounded;
            isGrounded = _groundChecker.IsGrounded;
            isOnSlope = _groundChecker.IsOnSlope;
            if (lockVelocityX)
                SetVelocityX(0f);
            if (useTargetVelocity)
            {
                rb.velocity = targetVelocity;
                useTargetVelocity = false;
            }

            HandleMovement();
            ApplyJump();
            
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
        protected virtual void OnUpdate(){}
        
        

        private void HandleMovement()
        {
            if (isAttacking)
            {
                lockVelocityX = true;
                useTargetVelocity = false;
                return;
            }

            lockVelocityX = false;
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
        }

        private void HandleJumpInput()
        {
            if (Input.GetKeyDown(keyJump) && (isGrounded) && !isAttacking  )
            {
                jumpRequested = true;
            }
        }

        private void ApplyJump()
        {
            if (jumpRequested)
            {
                isJumping = true;
                rb.gravityScale = 1f;
                useTargetVelocity = false;
                SetVelocityY(jumpForce);
                anim.SetTrigger("Jump");
                isGrounded = false;
                jumpRequested = false;
            }
        }

        private void HandleAttackInput()
        {
            if (Input.GetKeyDown(keyAttack) && attackTimer <= 0f && !isAttacking && isGrounded)
            {
                attackTimer = attackCooldown;
                Attack();
            }
        }

        protected virtual void Attack()
        {
        }

        protected override void Die()
        {
            StopAllCoroutines();
            isAttacking = false;
            lockVelocityX = false;
            inputEnabled = false;
            base.Die();
        }

        
    }