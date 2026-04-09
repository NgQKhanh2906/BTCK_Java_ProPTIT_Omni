using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SwordmanController : PlayerBase
{

    [Header("Attack Range")] 
    [SerializeField] private Transform atttackPoint1;
    [SerializeField] private Vector2 size1;
    [SerializeField] private Transform atttackPoint2;
    [SerializeField] private Vector2 size2;
    [SerializeField] private Transform atttackPoint3;
    [SerializeField] private Vector2 size3;
    

    [Header("Sword settings")] 
    [SerializeField] private float attackDamage;
    [SerializeField] private float atkDuration;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float comboWindow = 1f;
    [SerializeField] private float spAtkDamage;


    private float chargeTimer = 0f;
    private bool isCharging = false;
    private int comboStep = 0;
    private float comboTimer = 0f;


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
        anim.SetInteger("ComboStep", step);
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(atkDuration);
        isAttacking = false;
    }
    private IEnumerator AirAttack()
    {
        isAttacking = true;
        anim.SetTrigger("AirAttack");
        yield return new WaitForSeconds(0.6f);
        isAttacking = false;

    }
    #endregion

    
    
    #region SpecialAttack
    private void HandleCharge()
    {
        if (!isGrounded || isDefending || isRolling) return;
        if (Input.GetKey(keySpAtk))
        {
            if (!isCharging && !isAttacking)
            {
                isCharging = true;
                chargeTimer = 0f;
                SetVelocityX(0f);
                SetVelocityY(0f);
                anim.SetBool("IsCharging", isCharging);
                isAttacking =  true;
            }
            if(isCharging) chargeTimer += Time.deltaTime;
        }

        if (Input.GetKeyUp(keySpAtk) && isCharging)
        {
            isCharging = false;
            SetVelocityX(0f);
            anim.SetBool("IsCharging", isCharging);
            if (chargeTimer >= 0.5f)
            {
                anim.SetTrigger("ReleaseSpAtk");
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
        OnAttackHit(atttackPoint3, size3, attackDamage*1.3f);
    }

    public void HitSpecial()
    {
        float curDmg = spAtkDamage;
        if (chargeTimer >= 2f) curDmg *= 2.5f;
        else if (chargeTimer >= 1f) curDmg *= (2f);
        OnAttackHit(atttackPoint3,  size3, curDmg);
        chargeTimer = 0f;
    }
    #endregion

    
    public void OnAttackHit(Transform atttackPoint, Vector2 size, float dmg)
    {
        if (atttackPoint == null) return;
        Collider2D[] hits = Physics2D.OverlapBoxAll(atttackPoint.position, size, 0f, enemyLayer);

        foreach (var hit in hits)
        {
            var entity = hit.GetComponent<Entity>();
            if (entity == null) continue;
            Vector2 dir = (hit.transform.position - transform.position).normalized;
            entity.TakeDamage(dmg, dir);
        }
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