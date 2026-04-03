using UnityEngine;
using System;

public class Entity : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHP = 100f;
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float knockbackForce = 3f;

    [SerializeField] protected float currentHP;
    protected int facingDir = 1;
    protected bool isDead;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;

    public Action<float, float> OnHPChanged;
    public Action OnDeath;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
    }

    public virtual void TakeDamage(float dmg, Vector2 hitDir)
    {
        if (isDead) return;

        currentHP = Mathf.Max(0, currentHP - dmg);
        OnHPChanged?.Invoke(currentHP, maxHP);

        rb.velocity = Vector2.zero;
        rb.AddForce(hitDir.normalized * knockbackForce, ForceMode2D.Impulse);

        if (currentHP <= 0)
            Die();
        else
            anim.SetTrigger("Hit");
    }

    protected virtual void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        anim.SetTrigger("Die");
        OnDeath?.Invoke();
    }

    protected void Flip()
    {
        facingDir *= -1;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    protected void SetVelocityX(float x) =>
        rb.velocity = new Vector2(x, rb.velocity.y);

    protected void SetVelocityY(float y) =>
        rb.velocity = new Vector2(rb.velocity.x, y);

    public float GetHPPercent() => currentHP / maxHP;
    public bool  IsDead()       => isDead;
}