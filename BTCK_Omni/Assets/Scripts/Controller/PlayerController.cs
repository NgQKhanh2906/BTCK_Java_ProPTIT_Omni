using UnityEngine;
using System.Collections;
using Microsoft.Win32.SafeHandles;

public class PlayerController : MonoBehaviour
{
    private bool grounded;
    
    [Header("Movement settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private string axisName = "Horizontal";
    
    
    [Header("Jump settings")]
    [SerializeField] private string jumpBtn = "Jump";
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 boxSize = new Vector2();
    
    
    
    
    [Header("Roll settings")]
    [SerializeField] private KeyCode rollKey = KeyCode.R;
    [SerializeField] private float rollForce;
    [SerializeField] private float rollDuration;
    [SerializeField] private float rollCooldown;
    private bool isRolling = false;
    private float lastRollTime;
    
    
    [Header("Attack  settings")]
    [SerializeField] private KeyCode atkKey = KeyCode.J;
    [SerializeField] private float atkDuration;
    private bool isAttacking = false;
    // private int atkCount = 0;
    // private float lastAtkTime;


    [Header("SpecialAttack settings")] [SerializeField]
    private KeyCode spAtkKey = KeyCode.K;
    [SerializeField] private float spAtkDuration;
    
    
    [Header("Defend settings")]
    [SerializeField] private KeyCode defKey = KeyCode.S;
    private bool  isDefending = false;
    
    private Animator animator;
    private Rigidbody2D rb;
    
    
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        grounded = IsGrounded();
        HandleMovement();
        HandleJump();
        HandleRoll();
        HandleAttack();
        HandleDefend();
        UpdateAnimation();
    }

    private void HandleMovement()
    {
        if (!grounded || isRolling || isAttacking || isDefending) return;
        float moveInput = Input.GetAxisRaw(axisName);
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void HandleJump()
    {
        if (isRolling || isAttacking) return;
        if (Input.GetButtonDown(jumpBtn) && grounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
    private bool IsGrounded() => Physics2D.OverlapBox(groundCheck.position, boxSize, 0f, groundLayer);
    

    private void HandleRoll()
    {
        if (Input.GetKeyDown(rollKey) && grounded && !isRolling && Time.time > lastRollTime + rollCooldown)
        {
            StartCoroutine(Roll());
        }
    }

    private IEnumerator Roll()
    {
        isRolling = true;
        lastRollTime = Time.time;
        animator.SetTrigger("Rolling");
        float direction = transform.localScale.x;
        rb.velocity = new Vector2(direction * rollForce, rb.velocity.y);
        yield return new WaitForSeconds(rollDuration);
        isRolling = false;
    }


    private void HandleAttack()
    {
        if (!isAttacking && !isRolling && !isDefending)
        {
            if (Input.GetKeyDown(atkKey))
                StartCoroutine(Attack());
            else if (Input.GetKeyDown(spAtkKey))
                StartCoroutine(SpAttack());
        }
    }

    private IEnumerator SpAttack()
    {
        isAttacking = true;
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetTrigger("SpAttacking");
        yield return new WaitForSeconds(spAtkDuration);
        isAttacking = false;
    }
    private IEnumerator Attack()
    {
        isAttacking = true;
        if(!IsGrounded()) rb.velocity = new Vector2(rb.velocity.x, 0);
        else rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetTrigger("Attacking");
        yield return new WaitForSeconds(atkDuration);
        isAttacking = false;
    }


    private void HandleDefend()
    {
        if (Input.GetKey(defKey) && grounded && !isAttacking && !isRolling)
        {
            isDefending = true;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else if (Input.GetKeyUp(defKey))
        {
            isDefending = false;
            animator.speed = 1;
        }
    }

    private void HoldDef()
    {
        if (isDefending)
        {
            animator.speed = 0;
        }
    }
    
    
    
    private void UpdateAnimation()
    {
        bool isRunning = Mathf.Abs((rb.velocity.x)) > 0.1f;
        bool isJumping = !IsGrounded();
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isRolling", isRolling);
        animator.SetBool("isDefending", isDefending);
    }
   

    // private void OnDrawGizmos()
    // {
    //     if (groundCheck != null)
    //     {
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawWireCube(groundCheck.position, boxSize);
    //     }
    // }










    





    
}