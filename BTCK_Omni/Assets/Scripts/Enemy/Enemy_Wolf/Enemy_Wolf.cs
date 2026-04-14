using UnityEngine;

// Tự động gắn Mắt và Vũ khí khi kéo script này vào Object
[RequireComponent(typeof(MeleeAttack))]
[RequireComponent(typeof(PlayerDetector))]
public class Enemy_Wolf : EnemyBase
{
    [Header("Wolf AI Settings")]
    [SerializeField] private float attackRange = 1.5f; 
    [SerializeField] private float chaseSpeedMultiplier = 1.5f; // Đuổi nhanh gấp 1.5 lần đi tuần
    [SerializeField] private LedgeDetector ledgeCheck; // Gậy dò vực

    private MeleeAttack meleeWeapon;
    private PlayerDetector eyes;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.2f;
    private float knockbackTimer;

    protected override void Awake()
    {
        base.Awake();
        // Lấy các bộ phận đã được lắp trên cơ thể
        meleeWeapon = GetComponent<MeleeAttack>();
        eyes = GetComponent<PlayerDetector>();
    }

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        base.TakeDamage(dmg, hitDir);
        if (!isDead) knockbackTimer = knockbackDuration;
    }

    protected override void Update()
    {
        if (isDead) return;

        // 1. Xử lý Knockback (Bị chém lùi lại thì tạm thời không suy nghĩ)
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            return;
        }

        // 2. Chống trượt Patin: Khóa di chuyển khi đang diễn hoạt cảnh cắn
        // LƯU Ý: Sửa chữ "Wolf_attack" thành ĐÚNG TÊN cục State cắn trong Animator của bạn
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Wolf_attack"))
        {
            SetVelocityX(0);
            return;
        }

        // 3. LOGIC MẮT THẤY PLAYER (Đã tự động bỏ qua nếu vướng Tường)
        if (eyes.CanSeePlayer())
        {
            Transform target = eyes.GetPlayerTransform();
            float distanceToPlayer = Vector2.Distance(transform.position, target.position);

            // A. Nếu Player ở đủ gần -> Dừng lại và Cắn
            if (distanceToPlayer <= attackRange)
            {
                SetVelocityX(0); 
                anim.SetBool(animIsMoving, false); 
                
                // Ra lệnh cắn (Việc tính Cooldown và SetTrigger Animation giờ do MeleeWeapon tự lo)
                meleeWeapon.TryAttack(); 
            }
            // B. Nếu Player ở xa -> Rượt đuổi
            else
            {
                ChasePlayer(target);
            }
        }
        // 4. LOGIC MẤT DẤU PLAYER (Hoặc Player nấp sau tường) -> Đi tuần lại
        else
        {
            base.Update(); 
        }
    }

    private void ChasePlayer(Transform target)
    {
        // Kiểm tra địa hình: Nếu đụng Tường hoặc Thấy Vực -> KHÔNG ĐƯỢC LAO TỚI
        if (IsWallDetected() || (ledgeCheck != null && ledgeCheck.IsDetectingLedge()))
        {
            SetVelocityX(0); // Phanh gấp lại
            anim.SetBool(animIsMoving, false); // Đứng gầm gừ chờ Player sang
            
            // Dù không sang được nhưng vẫn quay mặt nhìn theo Player
            float dirToPlayer = target.position.x - transform.position.x;
            if ((dirToPlayer > 0 && facingDir == -1) || (dirToPlayer < 0 && facingDir == 1))
            {
                Flip();
            }
        }
        else // Đường xá an toàn -> Rượt thôi!
        {
            isIdle = false; // Ngắt trạng thái đứng nghỉ của Base
            anim.SetBool(animIsMoving, true);

            float directionToPlayer = target.position.x - transform.position.x;
            int moveDir = directionToPlayer > 0 ? 1 : -1;
            
            if (moveDir != facingDir) Flip();
            
            SetVelocityX(moveSpeed * chaseSpeedMultiplier * facingDir);
        }
    }
}