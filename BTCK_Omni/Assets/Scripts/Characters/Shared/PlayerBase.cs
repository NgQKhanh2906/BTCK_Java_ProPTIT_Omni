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

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    protected bool isGrounded;
    protected bool isAttacking = false;
    private bool inputEnabled  = true;
    private bool jumpRequested = false;
    private float attackTimer  = 0f;

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
            keyLeft   = KeyCode.A;
            keyRight  = KeyCode.D;
            keyJump   = KeyCode.W;
            keyAttack = KeyCode.J;
        }
        else
        {
            keyLeft   = KeyCode.LeftArrow;
            keyRight  = KeyCode.RightArrow;
            keyJump   = KeyCode.UpArrow;
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
        anim.SetBool ("IsGrounded", isGrounded);
        anim.SetFloat("VelocityY", rb.velocity.y);
    }

    private void FixedUpdate()
    {
        isGrounded= Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        HandleMovement();
        ApplyJump();
    }

    private void HandleMovement()
    {
        if (isAttacking)
        {
            SetVelocityX(0f);
            return;
        }
        
        float dir = 0f;
        if (Input.GetKey(keyLeft)) dir = -1f;
        if (Input.GetKey(keyRight)) dir =  1f;

        SetVelocityX(dir * moveSpeed);

        if (dir != 0 && dir != facingDir)
            Flip();
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(keyJump) && isGrounded && !isAttacking)
        {
            jumpRequested = true;
        }
    }

    private void ApplyJump()
    {
        if (jumpRequested)
        {
            SetVelocityY(jumpForce);
            anim.SetTrigger("Jump");
            jumpRequested = false;
        }
    }

    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(keyAttack) && attackTimer <= 0f && !isAttacking)
        {
            attackTimer = attackCooldown;
            Attack();
        }
    }

    
    protected virtual void Attack()
    {
    }
    
    public void SetInputEnabled(bool val) => inputEnabled = val;

    protected override void Die()
    {
        StopAllCoroutines();
        isAttacking  = false;
        inputEnabled = false;
        base.Die();
    }

    //
    // private void OnDrawGizmosSelected()
    // {
    //     //if (groundCheck == null) return;
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    // }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}