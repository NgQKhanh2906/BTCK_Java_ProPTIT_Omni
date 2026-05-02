using UnityEngine;

public class IceCone : MonoBehaviour
{
    [Header("Ice Block Settings")]
    [SerializeField] private float speed = 10f;
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
                Vector2 knockDir = new Vector2(direction, 0).normalized;
                entity.TakeDamage(damage, knockDir);

                if (hitEffectPrefab != null)
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
            else 
            {
                if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground", "Wall")) != 0)
                {
                    if (hitEffectPrefab != null)
                        Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                        
                    Destroy(gameObject);
                }
            }
        }
    }
}