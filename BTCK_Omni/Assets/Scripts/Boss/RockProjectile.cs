using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public float dmg = 20f;

    private Vector2 moveDir;
    private float speed;
    private bool hasHit = false;

    public void Setup(Vector2 dir, float s)
    {
        moveDir = dir.normalized;
        speed = s;
        Invoke("DestroyRock", 5f);
    }

    void Update()
    {
        if (hasHit) return;

        float dist = speed * Time.deltaTime;
        LayerMask mask = playerLayer | groundLayer;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, dist, mask);

        if (hit.collider != null)
        {
            transform.position = hit.point;
            int l = hit.collider.gameObject.layer;

            if (((1 << l) & playerLayer) != 0)
            {
                PlayerBase p = hit.collider.GetComponent<PlayerBase>();

                if (p != null) p.TakeDamage(dmg, moveDir);

                hasHit = true;
                DestroyRock();
            }
            else if (((1 << l) & groundLayer) != 0)
            {
                hasHit = true;
                DestroyRock();
            }
        }
        else
        {
            transform.Translate(moveDir * dist, Space.World);
        }
    }

    void DestroyRock()
    {
        Destroy(gameObject);
    }
}