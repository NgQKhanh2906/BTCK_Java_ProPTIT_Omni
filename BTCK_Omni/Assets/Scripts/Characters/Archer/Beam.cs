using UnityEngine;

public class Beam : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private float maxL = 30f;
    [SerializeField] private float time = 0.4f;

    public void Setup(float dir, float d)
    {
        Vector2 p = transform.position;
        Vector2 f = new Vector2(dir, 0);
        float l = maxL;

        RaycastHit2D hitG = Physics2D.Raycast(p, f, maxL, groundMask);

        if (hitG.collider != null)
        {
            l = hitG.distance;
        }

        RaycastHit2D[] hitEs = Physics2D.RaycastAll(p, f, l, enemyMask);

        foreach (RaycastHit2D hit in hitEs)
        {
            Entity e = hit.collider.GetComponent<Entity>();

            if (e != null)
            {
                e.TakeDamage(d, f);
            }
        }

        transform.right = f;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float w = sr.sprite.bounds.size.x;

        transform.localScale = new Vector3(l / w, 1f, 1f);

        Destroy(gameObject, time);
    }
}