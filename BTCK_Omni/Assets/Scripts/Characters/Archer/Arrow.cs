using UnityEngine;
using UnityEngine.Pool;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    [SerializeField] private float turnSpd = 15f;
    [SerializeField] private float scanR = 30f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;
    public AudioClip hitSound;

    private float damage;
    private int direction;
    private int type;
    private bool hasHit = false;
    private Vector3 moveDir;
    private bool isAtk3;
    private Transform tgt;

    private Animator anim;
    private Collider2D col2D;
    private Rigidbody2D rb;

    // Pool
    private IObjectPool<Arrow> pool;
    public void SetPool(IObjectPool<Arrow> p) => pool = p;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col2D = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(int dir, float dmg, int arrowType, float ang = 0f, bool atk3 = false)
    {
        // Quan trọng: Reset trạng thái
        CancelInvoke();
        hasHit = false;
        tgt = null;
        if (col2D != null) col2D.enabled = true; // Bật lại collider

        direction = dir;
        damage = dmg;
        type = arrowType;
        isAtk3 = atk3;

        anim.SetBool("isAtk3", isAtk3);
        transform.rotation = Quaternion.Euler(0, 0, direction == 1 ? ang : 180 - ang);

        Vector3 s = transform.localScale;
        s.y = direction == 1 ? Mathf.Abs(s.y) : -Mathf.Abs(s.y);
        transform.localScale = s;

        moveDir = transform.right;

        if (isAtk3)
        {
            Collider2D[] arr = Physics2D.OverlapCircleAll(transform.position, scanR, enemyLayer);
            float min = Mathf.Infinity;
            foreach (Collider2D c in arr)
            {
                Vector2 dirTo = c.transform.position - transform.position;
                if ((direction > 0 && dirTo.x > 0) || (direction < 0 && dirTo.x < 0))
                {
                    float d = dirTo.sqrMagnitude;
                    if (d < min) { min = d; tgt = c.transform; }
                }
            }
        }
        Invoke(nameof(DestroyArrow), lifeTime);
    }

    private void Update()
    {
        if (hasHit) return;

        if (isAtk3 && tgt != null)
        {
            Vector2 tDir = (tgt.position - transform.position).normalized;
            moveDir = Vector3.Lerp(moveDir, tDir, turnSpd * Time.deltaTime).normalized;
            float a = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(a, Vector3.forward);
            Vector3 scl = transform.localScale;
            scl.y = moveDir.x < 0 ? -Mathf.Abs(scl.y) : Mathf.Abs(scl.y);
            transform.localScale = scl;
        }

        float d = speed * Time.deltaTime;
        LayerMask mask = enemyLayer | groundLayer;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, d, mask);

        if (hit.collider != null)
        {
            transform.position = hit.point;
            hasHit = true;

            if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
            {
                Entity e = hit.collider.GetComponent<Entity>();
                if (e != null) e.TakeDamage(damage, new Vector2(direction, 0));
            }

            if (hitSound != null)
            {
                float v = AudioManager.instance != null ? AudioManager.instance.soundEffectsVolume : 1f;
                GameObject o = new GameObject("HitSnd");
                o.transform.position = transform.position;
                AudioSource src = o.AddComponent<AudioSource>();
                src.clip = hitSound;
                src.volume = v * 2f;
                src.spatialBlend = 0f;
                src.Play();
                Destroy(o, hitSound.length);
            }

            CancelInvoke(nameof(DestroyArrow));
            if (col2D != null) col2D.enabled = false;
            if (rb != null) rb.velocity = Vector2.zero;

            if (isAtk3)
            {
                transform.rotation = Quaternion.identity;
                Vector3 s = transform.localScale;
                s.x = direction; s.y = Mathf.Abs(s.y);
                transform.localScale = s;
            }

            if (anim != null)
            {
                anim.SetInteger("ArrowType", type);
                anim.SetTrigger("Hit");
                anim.Update(0f);
            }
            Invoke(nameof(DestroyArrow), 1.5f);
        }
        else
        {
            transform.Translate(moveDir * d, Space.World);
        }
    }

    public void DestroyArrow()
    {
        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }
}