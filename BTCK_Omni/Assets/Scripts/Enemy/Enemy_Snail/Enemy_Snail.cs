using UnityEngine;

public class Enemy_Snail : EnemyBase
{
    [Header("Snail Logic")]
    [SerializeField] private bool isHiding = false;

    [Header("Player Detection")]
    [SerializeField] private Transform playerCheck; 
    [SerializeField] private float detectionRange = 5f; 
    [SerializeField] private LayerMask whatIsPlayer; 

    [Header("Animation Timers")]
    [SerializeField] private float wakeUpDuration = 0.5f; // Thời gian chờ chui ra khỏi vỏ
    private float wakeUpTimer;

    private bool isPlayerFound;

    protected override void Awake()
    {
        base.Awake();
        facingDir = -1; // Sửa lỗi lùi
    }

    protected override void Update()
    {
        if (isDead) return;

        // --- PHẦN MỚI THÊM: Đứng im chờ chui ra xong mới được đi ---
        if (wakeUpTimer > 0)
        {
            wakeUpTimer -= Time.deltaTime;
            SetVelocityX(0); // Bắt buộc đứng im
            return; // Khóa không cho chạy các logic bên dưới
        }
        // -------------------------------------------------------------

        bool wasDetected = isPlayerFound;

        // Tự kiểm tra Player (Bằng vòng tròn đỏ)
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
        // PHẦN MỚI THÊM: Bất tử cả lúc đang cuộn tròn VÀ lúc đang lồm cồm chui ra
        if (isHiding || wakeUpTimer > 0) 
        {
            Debug.Log("Snail: Đang trong vỏ hoặc đang chui ra, bất tử nhé!");
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
        
        // PHẦN MỚI THÊM: Bắt đầu bấm giờ đợi animation chui ra diễn xong
        wakeUpTimer = wakeUpDuration; 
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