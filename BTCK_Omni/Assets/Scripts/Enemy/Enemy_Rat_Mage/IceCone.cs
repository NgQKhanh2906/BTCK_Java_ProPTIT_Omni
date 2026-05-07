using UnityEngine;

public class IceCone : MonoBehaviour
{
    [Header("Ice Block Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private LayerMask hitMask; 
    
    [Header("Effects & Audio")]
    [SerializeField] private GameObject hitEffectPrefab; 
    [Tooltip("File âm thanh khi băng vỡ")]
    [SerializeField] private AudioClip hitSound; 
    [Range(0f, 1f)] [SerializeField] private float hitVolume = 0.8f; // Chỉnh âm lượng từ 0 đến 1

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

        Destroy(gameObject, 4f); 
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(speed * direction, rb.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) return;

        if (((1 << collision.gameObject.layer) & hitMask) != 0)
        {
            Entity entity = collision.GetComponentInParent<Entity>();
            
            if (entity != null)
            {
                // Trúng người chơi
                Vector2 knockDir = new Vector2(direction, 0).normalized;
                entity.TakeDamage(damage, knockDir);

                PlayHitEffects();
                Destroy(gameObject);
            }
            else 
            {
                // Chạm vào Tường/Đất
                if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground", "Wall")) != 0)
                {
                    PlayHitEffects();
                    Destroy(gameObject);
                }
            }
        }
    }

    // Hàm phụ để code gọn gàng hơn
    private void PlayHitEffects()
    {
        // 1. Sinh ra hiệu ứng hình ảnh (Particle/Animation)
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. Phát âm thanh (SFX)
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
        }
    }
}