using UnityEngine;
using System;
using System.Collections;

public class Entity : MonoBehaviour, IDamageable
{
    [Header("Stats")] [SerializeField] protected float maxHP = 100f;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float knockbackForce;
    protected float currentHP;


    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;


    protected int facingDir = 1;
    protected bool isDead;
    protected bool isInvincible;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;

    public event Action<float, float> OnHPChanged;
    public event Action OnDeath;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
        isDead = false;
    }

    protected void NotifyHPChanged()
    {
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    protected void NotifyDeath()
    {
        OnDeath?.Invoke();
    }

    public virtual void TakeDamage(float dmg, Vector2 hitDir)
    {
        if (isDead || isInvincible) return;
        currentHP = Mathf.Max(0, currentHP - dmg);
        NotifyHPChanged();
        rb.velocity = Vector2.zero;
        rb.AddForce(hitDir.normalized * knockbackForce, ForceMode2D.Impulse);
        if (currentHP <= 0)
            Die();
        else
            anim.SetTrigger(GameConfig.ANIM_COL_HIT);
    }
    
    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        gameObject.layer = LayerMask.NameToLayer("Corpse");
        anim.SetTrigger(GameConfig.ANIM_COL_DIE);
        NotifyDeath();
        StartCoroutine(FinalizeDeathPhysics());
    }
    
    private IEnumerator FinalizeDeathPhysics()
    {
        float timeout = 2f;
        float elapsed = 0f;
        while (elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            if (rb.velocity.y >= -0.1f && elapsed > 0.3f) break;
            yield return null;
        }
        rb.velocity = Vector2.zero;
        rb.isKinematic  = true;
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
    public bool IsDead() => isDead;
}