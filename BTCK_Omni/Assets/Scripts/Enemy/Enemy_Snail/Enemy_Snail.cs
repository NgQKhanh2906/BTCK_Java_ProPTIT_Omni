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

    // [ĐÃ SỬA] Hàm nhận sát thương
    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        // 1. NẾU ĐANG TRỐN TRONG VỎ
        if (isHiding)
        {
            // Giảm 90% sát thương (tức là chỉ nhận 10%)
            float reducedDamage = dmg * 0.1f; 
            
            // Xóa bỏ lực hất văng (Knockback = 0)
            Vector2 noKnockback = Vector2.zero; 

            // Gửi sát thương đã giảm và lực hất = 0 lên lớp cha (Entity/EnemyBase) để xử lý trừ máu
            base.TakeDamage(reducedDamage, noKnockback);
            
            // [Tùy chọn] Phát một âm thanh đặc biệt (tiếng gõ keng keng vào mai rùa) ở đây
            // PlayTingSound();

            return; // Dừng lại ở đây, không chạy các lệnh bên dưới nữa
        }

        // 2. NẾU ĐANG BÒ BÌNH THƯỜNG (Không trốn)
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
            SetVelocityX(0); // Dừng lại ngay lập tức
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