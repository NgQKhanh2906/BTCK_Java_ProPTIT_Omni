using UnityEngine;

public class CyclopsBeam : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private LayerMask hitMask; 

    [Header("Effects & Audio")]
    [SerializeField] private GameObject hitEffectPrefab; 
    [SerializeField] private AudioClip laserHitSFX; 
    [SerializeField] [Range(0f, 1f)] private float hitVolume = 1f;

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
        if (collision.CompareTag("Enemy")) return;
        
        if (((1 << collision.gameObject.layer) & hitMask) != 0)
        {
            Entity entity = collision.GetComponentInParent<Entity>();
            
            if (entity != null)
            {
                Vector2 knockDir = new Vector2(direction, 0).normalized;
                entity.TakeDamage(damage, knockDir);
                PlayHitSound();
                
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
            else 
            {
                if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground", "Wall")) != 0)
                {
                    PlayHitSound();
                    
                    if (hitEffectPrefab != null)
                    {
                        Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                    }
                    Destroy(gameObject);
                }
            }
        }
    }

    private void PlayHitSound()
    {
        if (laserHitSFX != null)
        {
            AudioSource.PlayClipAtPoint(laserHitSFX, transform.position, hitVolume);
        }
    }
}