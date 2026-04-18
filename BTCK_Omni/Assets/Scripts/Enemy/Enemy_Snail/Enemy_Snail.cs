using UnityEngine;

[RequireComponent(typeof(PlayerDetector))]
public class Enemy_Snail : EnemyBase
{
    [Header("Snail Logic")]
    [SerializeField] private bool isHiding = false;

    [Header("Animation Timers")]
    [SerializeField] private float wakeUpDuration = 0.5f; 
    private float wakeUpTimer;

    private PlayerDetector eyes; 

    protected override void Awake()
    {
        base.Awake();
        eyes = GetComponent<PlayerDetector>();
    }

    protected override void Update()
    {
        if (isDead) return;
        if (wakeUpTimer > 0)
        {
            wakeUpTimer -= Time.deltaTime;
            SetVelocityX(0); 
            return; 
        }
        if (isHiding)
        {
            SetVelocityX(0);

            if (!eyes.CanSeePlayer()) 
            {
                Debug.Log("<color=green>Snail: an toàn rồi, chui ra thôi.</color>");
                ExitHideMode();
            }
            return;
        }
        base.Update();
    }

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        if (isHiding || wakeUpTimer > 0) 
        {
            Debug.Log("Snail: Đang trong vỏ, leng keng, không mất máu!");
            return;
        }
        base.TakeDamage(dmg, hitDir);
        if (!isDead) 
        {
            EnterHideMode();
        }
    }

    private void EnterHideMode()
    {
        isHiding = true;
        anim.SetBool(animIsHiding, true); 
    }

    private void ExitHideMode()
    {
        isHiding = false;
        anim.SetBool(animIsHiding, false);
    
        wakeUpTimer = wakeUpDuration; 
    }
}