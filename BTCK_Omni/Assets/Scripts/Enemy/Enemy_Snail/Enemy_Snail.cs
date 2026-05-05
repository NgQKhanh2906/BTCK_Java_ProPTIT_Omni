using UnityEngine;

public class Enemy_Snail : EnemyBase
{
    [Header("Snail Settings")]
    [SerializeField] private bool isHiding = false;
    private readonly int hashExitHideState = Animator.StringToHash("ExitHide");
    private readonly int animIsHidingParam = Animator.StringToHash("isHiding");

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        if (isDead) return;
        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.shortNameHash == hashExitHideState)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            return;
        }
        
        Transform target = GetVisiblePlayer();
        
        if (isHiding)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            if (target == null)
            {
                ExitHideMode();
            }
            return; 
        }
        
        float distToHome = Mathf.Abs(transform.position.x - startPosition.x);
        if (distToHome > (maxWanderDistance + 0.5f) && !isIdle)
        {
            isReturningHome = true;
        }
        
        base.Update();
    }

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        if (isHiding)
        {
            float reducedDamage = dmg * 0.1f; 
            Vector2 noKnockback = Vector2.zero; 
            base.TakeDamage(reducedDamage, noKnockback);

            return; 
        }
        base.TakeDamage(dmg, hitDir);

        if (isDead) return;
        
        Transform target = GetVisiblePlayer();
        
        if (target != null)
        {
            EnterHideMode();
        }
    }

    private void EnterHideMode()
    {
        if (!isHiding)
        {
            isHiding = true;
            anim.SetBool(animIsHidingParam, true);
            SetVelocityX(0); 
        }
    }

    private void ExitHideMode()
    {
        if (isHiding)
        {
            isHiding = false;
            anim.SetBool(animIsHidingParam, false);
        }
    }
}