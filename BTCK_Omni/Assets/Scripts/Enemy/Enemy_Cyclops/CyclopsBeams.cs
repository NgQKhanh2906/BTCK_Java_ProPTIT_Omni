using UnityEngine;

public class CyclopsBeam : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private LayerMask hitMask; 
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab; 

    private float damage;
    private int direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(int dir, float dmg)
    {
        direction = dir;
        damage = dmg;

        if (direction < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        Destroy(gameObject, 3f); 
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(speed * direction, rb.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Bỏ qua va chạm với chính quái vật
        if (collision.CompareTag("Enemy")) return;

        // Nếu chạm vào mục tiêu nằm trong hitMask (Player, Ground, Wall...)
        if (((1 << collision.gameObject.layer) & hitMask) != 0)
        {
            // SỬA Ở ĐÂY: Dùng GetComponentInParent thay vì GetComponent
            // Để dù laser bắn trúng "Bàn chân" (GroundCheck) thì vẫn tìm được script ở "Thân"
            Entity entity = collision.GetComponentInParent<Entity>();
            
            if (entity != null)
            {
                // Trừ máu và truyền hướng knockback
                Vector2 knockDir = new Vector2(direction, 0).normalized;
                entity.TakeDamage(damage, knockDir);

                // Sinh ra hiệu ứng máu/chớp nháy (nếu có)
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }

                // Hủy đạn
                Destroy(gameObject);
            }
            else 
            {
                // Lọc trường hợp: Đạn chạm vào Tường / Mặt đất (Layer Ground) thì nổ
                // Còn chạm vào những thứ linh tinh khác của Player mà ko có Entity thì xuyên qua luôn
                if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground", "Wall")) != 0)
                {
                    if (hitEffectPrefab != null)
                    {
                        Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                    }
                    Destroy(gameObject);
                }
            }
        }
    }
}