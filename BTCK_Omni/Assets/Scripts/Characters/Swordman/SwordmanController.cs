using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SwordmanController : PlayerBase
{
    [Header("Attack Range")] [SerializeField]
    private Transform atttackPoint1;

    [SerializeField] private Vector2 size1;
    [SerializeField] private Transform atttackPoint2;
    [SerializeField] private Vector2 size2;
    [SerializeField] private Transform atttackPoint3;
    [SerializeField] private Vector2 size3;


    [Header("Sword settings")] [SerializeField]
    private float attackDamage;

    //[SerializeField] private float atkDuration;
    [SerializeField] private float comboWindow = 1f;
    [SerializeField] private float spAtkDamage;

    private static readonly WaitForSeconds atkWait = new WaitForSeconds(0.7f);
    private static readonly WaitForSeconds airAtkWait = new WaitForSeconds(0.6f);
    private int hitBufferSize = 16;
    private Collider2D[] hitBuffer;

    private float chargeTimer = 0f;
    private bool isCharging = false;
    private int comboStep = 0;
    private float comboTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        
        hitBuffer = new Collider2D[Mathf.Max(1, hitBufferSize)];
    }

    protected override void OnUpdate()
    {
        HandleCharge();
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                comboStep = 0;
        }
    }

    #region NormalAttack

    protected override void Attack(bool hasUsedAirAttack)
    {
        if (isGrounded)
        {
            comboStep = (comboStep % 3) + 1;
            comboTimer = comboWindow;

            StartCoroutine(SwordAttackRoutine(comboStep));
        }
        else if (hasUsedAirAttack)
        {
            StartCoroutine(AirAttack());
        }
    }

    private IEnumerator SwordAttackRoutine(int step)
    {
        isAttacking = true;
        SetVelocityX(0);
        anim.SetInteger(GameConfig.ANIM_COL_COMBO_STEP, step);
        anim.SetTrigger(GameConfig.ANIM_COL_ATTACK);
        yield return atkWait;
        isAttacking = false;
    }

    private IEnumerator AirAttack()
    {
        isAttacking = true;
        anim.SetTrigger(GameConfig.ANIM_COL_AIRATTACK);
        yield return airAtkWait;
        isAttacking = false;
    }

    #endregion


    #region SpecialAttack

    private void HandleCharge()
    {
        if (!isGrounded || isDefending || isRolling) return;
        if (Input.GetKey(keySpAtk) && CurrentMana >= MaxMana)
        {
            if (!isCharging && !isAttacking)
            {
                isCharging = true;
                chargeTimer = 0f;
                SetVelocityX(0f);
                SetVelocityY(0f);
                anim.SetBool(GameConfig.ANIM_COL_ISCHARGING, isCharging);
                isAttacking = true;
            }

            if (isCharging) chargeTimer += Time.deltaTime;
        }

        if (Input.GetKeyUp(keySpAtk) && isCharging)
        {
            isCharging = false;
            SetVelocityX(0f);
            anim.SetBool(GameConfig.ANIM_COL_ISCHARGING, isCharging);
            if (chargeTimer >= 0.5f)
            {
                anim.SetTrigger(GameConfig.ANIM_COL_RELEASE_SPATK);
                RestoreMana(-CurrentMana);
            }
            else
            {
                isAttacking = false;
                chargeTimer = 0f;
            }
        }
    }

    private void EndSpecialAttack()
    {
        isAttacking = false;
    }

    #endregion


    #region AnimationEventForAttack

    public void Hit1()
    {
        OnAttackHit(atttackPoint1, size1, attackDamage);
    }

    public void Hit2()
    {
        OnAttackHit(atttackPoint2, size2, attackDamage * 0.9f);
    }

    public void Hit3()
    {
        OnAttackHit(atttackPoint3, size3, attackDamage * 1.3f);
    }

    public void HitSpecial()
    {
        float curDmg = spAtkDamage;
        if (chargeTimer >= 2f) curDmg *= 2.5f;
        else if (chargeTimer >= 1f) curDmg *= (2f);
        OnAttackHit(atttackPoint3, size3, curDmg);
        chargeTimer = 0f;
    }

    #endregion


    public void OnAttackHit(Transform atttackPoint, Vector2 size, float dmg)
    {
        if (atttackPoint == null) return;
        if (hitBuffer == null || hitBuffer.Length != Mathf.Max(1, hitBufferSize))
            hitBuffer = new Collider2D[Mathf.Max(1, hitBufferSize)];
        int hitCount = Physics2D.OverlapBoxNonAlloc(atttackPoint.position, size, 0f, hitBuffer, enemyLayerMask);
        bool hasHit = false;
        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit == null) continue;
            var entity = hit.GetComponent<Entity>();
            if (entity == null) continue;
            Vector2 dir = (hit.transform.position - transform.position).normalized;
            entity.TakeDamage(dmg, dir);
            hasHit = true;
        }

        if (hasHit) RestoreMana(manaPerHit);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (atttackPoint1 != null)
        {
            Gizmos.DrawWireCube(atttackPoint1.position, size1);
        }

        Gizmos.color = Color.green;
        if (atttackPoint2 != null)
        {
            Gizmos.DrawWireCube(atttackPoint2.position, size2);
        }

        Gizmos.color = Color.blue;
        if (atttackPoint3 != null)
        {
            Gizmos.DrawWireCube(atttackPoint3.position, size3);
        }
    }
    
}