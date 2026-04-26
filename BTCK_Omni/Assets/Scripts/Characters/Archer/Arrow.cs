using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;

    [Header("Collision Layers")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;

    private float damage;
    private int direction;
    private int type;
    private bool hasHit = false;
    private Vector3 moveDir;

    private Animator anim;
    private Collider2D col2D;
    private Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col2D = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(int dir, float dmg, int arrowType, float ang = 0f, bool isAtk3 = false)
    {
        direction = dir;
        damage = dmg;
        type = arrowType;

        anim.SetBool("isAtk3", isAtk3);

        transform.rotation = Quaternion.Euler(0, 0, direction == 1 ? ang :  180 - ang);
        moveDir = transform.right;

        Invoke(nameof(DestroyArrow), lifeTime);
    }

    private void Update()
    {
        if (hasHit)
        {
            return;
        }

        Move();
    }

    private void Move()
    {
        float distanceThisFrame = speed * Time.deltaTime;
        LayerMask hitMask = enemyLayer | groundLayer;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, distanceThisFrame, hitMask);

        if (hit.collider != null)
        {
            transform.position = hit.point;

            if (IsLayerMatch(hit.collider.gameObject.layer, enemyLayer))
            {
                HitEnemy(hit.collider);
            }
            else if (IsLayerMatch(hit.collider.gameObject.layer, groundLayer))
            {
                HitGround();
            }
        }
        else
        {
            transform.Translate(moveDir * distanceThisFrame, Space.World);
        }
    }

    private bool IsLayerMatch(int layer, LayerMask layerMask)
    {
        return ((1 << layer) & layerMask) != 0;
    }

    private void HitEnemy(Collider2D col)
    {
        hasHit = true;

        Entity e = col.GetComponent<Entity>();
        if (e != null)
        {
            e.TakeDamage(damage, new Vector2(direction, 0));
        }

        TriggerHitEffect();
    }

    private void HitGround()
    {
        hasHit = true;
        TriggerHitEffect();
    }

    private void TriggerHitEffect()
    {
        CancelInvoke(nameof(DestroyArrow));

        if (col2D != null)
        {
            col2D.enabled = false;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            //rb.isKinematic = true;
        }

        if (anim != null)
        {
            anim.SetInteger("ArrowType", type);
            anim.SetTrigger("Hit");
            anim.Update(0f);
        }

        Invoke(nameof(DestroyArrow), 1.5f);
    }

    public void DestroyArrow()
    {
        Destroy(gameObject);
    }
}