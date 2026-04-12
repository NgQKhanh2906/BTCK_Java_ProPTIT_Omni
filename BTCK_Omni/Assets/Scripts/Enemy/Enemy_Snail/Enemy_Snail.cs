using UnityEngine;

public class Enemy_Snail : EnemyBase
{
    [Header("Snail Logic")]
    [SerializeField] private bool isHiding = false;

    [Header("Player Detection")]
    [SerializeField] private Transform playerCheck; 
    [SerializeField] private float detectionRange = 5f; 
    [SerializeField] private LayerMask whatIsPlayer; 

    private bool isPlayerFound; // Tên biến mới để tránh trùng lặp

    protected override void Awake()
    {
        base.Awake();
        facingDir = -1; 
    }
    protected override void Update()
    {
        if (isDead) return;

        bool wasDetected = isPlayerFound;

        // Tự kiểm tra Player
        if (playerCheck != null)
        {
            isPlayerFound = Physics2D.OverlapCircle(playerCheck.position, detectionRange, whatIsPlayer);
        }

        // Debug Log thông minh
        if (isPlayerFound != wasDetected)
        {
            if (isPlayerFound) Debug.Log("<color=red>Snail: Thấy Player rồi, chuẩn bị trốn!</color>");
            else Debug.Log("<color=green>Snail: an toàn rồi, chui ra thôi.</color>");
        }

        // Logic ẩn nấp
        if (isHiding)
        {
            SetVelocityX(0);
            if (!isPlayerFound) ExitHideMode();
            return;
        }

        base.Update();
    }

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        if (isHiding) 
        {
            Debug.Log("Snail: Đang trong vỏ, bất tử nhé!");
            return;
        }

        base.TakeDamage(dmg, hitDir);

        if (!isDead) EnterHideMode();
    }

    private void EnterHideMode()
    {
        isHiding = true;
        anim.SetBool("isHiding", true);
    }

    private void ExitHideMode()
    {
        isHiding = false;
        anim.SetBool("isHiding", false);
    }

    private void OnDrawGizmos()
    {
        if (playerCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerCheck.position, detectionRange);
        }
    }
}