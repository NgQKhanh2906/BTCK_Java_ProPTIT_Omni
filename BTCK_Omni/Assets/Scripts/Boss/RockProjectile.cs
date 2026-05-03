using UnityEngine;
using UnityEngine.Pool;

public class RockProjectile : MonoBehaviour
{
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public float dmg = 20f;

    [Header("SFX")]
    public AudioClip shootSound;
    public AudioClip hitSound;

    private Vector2 moveDir;
    private float speed;
    private bool hasHit = false;

    private IObjectPool<RockProjectile> pool;
    public void SetPool(IObjectPool<RockProjectile> p) => pool = p;

    public void Setup(Vector2 dir, float s)
    {
        CancelInvoke();
        hasHit = false;

        moveDir = dir.normalized;
        speed = s;

        PlaySfx(shootSound);

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
                PlaySfx(hitSound);
                DestroyRock();
            }
            else if (((1 << l) & groundLayer) != 0)
            {
                hasHit = true;
                PlaySfx(hitSound);
                DestroyRock();
            }
        }
        else
        {
            transform.Translate(moveDir * dist, Space.World);
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            float v = 1f;
            if (AudioManager.instance != null)
            {
                v = AudioManager.instance.soundEffectsVolume;
            }

            AudioSource.PlayClipAtPoint(clip, transform.position, v);
        }
    }

    void DestroyRock()
    {
        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }
}