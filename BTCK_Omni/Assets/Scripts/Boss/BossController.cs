using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Pool;

public class BossController : Entity
{
    [SerializeField] private Material flashMat;
    [SerializeField] private float flashDuration = 0.15f;
    private Material defaultMat;

    public GameObject rock;
    public GameObject laser;
    public Transform eyeP;
    public Transform handP;

    public float hp = 3000f;
    public float maxHp = 3000f;
    public LayerMask hitMask;

    public bool isFacingRight = false;
    Transform t;

    private ObjectPool<RockProjectile> rockPool;
    [Header("Laser SFX")]
    public AudioClip straightLaserSound; 
    public AudioClip sweepLaserSound;    


    protected override void Awake()
    {
        base.Awake();
        defaultMat = sr.material;

        rockPool = new ObjectPool<RockProjectile>(
            createFunc: () => {
                GameObject g = Instantiate(rock);
                RockProjectile rp = g.GetComponent<RockProjectile>();
                rp.SetPool(rockPool);
                return rp;
            },
            actionOnGet: (rp) => rp.gameObject.SetActive(true),
            actionOnRelease: (rp) => rp.gameObject.SetActive(false),
            actionOnDestroy: (rp) => Destroy(rp.gameObject),
            collectionCheck: false,
            defaultCapacity: 50,
            maxSize: 200
        );
    }

    void Start()
    {
        StartCoroutine(Loop());
    }

    void Update()
    {
        if (isDead) return;
        Find();
        LookAtPlayer();
    }

    void Find()
    {
        int pL = LayerMask.GetMask("Player");
        Collider2D[] ps = Physics2D.OverlapCircleAll(transform.position, 90f, pL);
        float min = Mathf.Infinity;
        t = null;

        foreach (Collider2D p in ps)
        {
            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d < min)
            {
                min = d;
                t = p.transform;
            }
        }
    }

    void LookAtPlayer()
    {
        if (t != null)
        {
            if (transform.position.x > t.position.x && !isFacingRight) FlipBoss();
            else if (transform.position.x < t.position.x && isFacingRight) FlipBoss();
        }
    }

    void FlipBoss()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        if (isDead) return;

        StartCoroutine(Flash());
        base.TakeDamage(dmg, hitDir);

        hp -= dmg;
        if (hp <= 0) Die();
    }

    private IEnumerator Flash()
    {
        sr.material = flashMat;
        yield return new WaitForSeconds(flashDuration);
        sr.material = defaultMat;
    }

    public override void Die()
    {
        base.Die();
        anim.SetTrigger("die");
        StopAllCoroutines();
        Invoke("EndGame", 2f);
    }

    void EndGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.Victory();
        else Debug.LogError("Không tìm thấy GameManager!");
    }

    IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            if (hp > 0 && t != null)
            {
                int r = Random.Range(1, 6);
                if (r == 1) anim.SetTrigger("atk1");
                else if (r == 2) anim.SetTrigger("atk2");
                else if (r == 3) anim.SetTrigger("atk3");
                else if (r == 4) anim.SetTrigger("def");
                else if (r == 5) anim.SetTrigger("heal");
            }
        }
    }

    public void F_Atk1()
    {
        if (t != null)
        {
            Vector2 d = (t.position - handP.position).normalized;
            float a = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;

            RockProjectile rp = rockPool.Get();
            rp.transform.position = handP.position;
            rp.transform.rotation = Quaternion.Euler(0, 0, a);
            rp.Setup(d, 15f);
        }
    }

    public void F_Atk2()
    {
        StartCoroutine(Sp2());
    }

    IEnumerator Sp2()
    {
        anim.speed = 0;
        int gl = LayerMask.GetMask("Ground");
        for (int i = 0; i < 30; i++)
        {
            float rx = transform.position.x + Random.Range(-90f, 90f);
            Vector2 rS = new Vector2(rx, transform.position.y + 20f);
            RaycastHit2D hit = Physics2D.Raycast(rS, Vector2.down, 40f, gl);

            if (hit.collider != null)
            {
                Vector3 p = new Vector3(rx, hit.point.y + 0.2f, 0);

                RockProjectile rp = rockPool.Get();
                rp.transform.position = p;
                rp.transform.rotation = Quaternion.Euler(0, 0, 90f);
                rp.Setup(Vector2.up, 15f);
            }
            yield return new WaitForSeconds(0.05f);
        }
        anim.speed = 1;
    }

    public void F_Atk3()
    {
        if (t != null)
        {
            Vector2 d = (t.position - eyeP.position).normalized;
            float a = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            Vector2 origin = (Vector2)eyeP.position + d * 1.2f;
            RaycastHit2D hit = Physics2D.Raycast(origin, d, 50f, hitMask);
            float len = 50f;
            if (eyeP != null) PlaySfx(straightLaserSound, eyeP.position);

            if (hit.collider != null)
            {
                len = hit.distance + 1.2f;
                if (((1 << hit.collider.gameObject.layer) & LayerMask.GetMask("Player")) != 0)
                {
                    PlayerBase p = hit.collider.GetComponent<PlayerBase>();
                    if (p != null) p.TakeDamage(15f, d);
                }
            }

            GameObject l = Instantiate(laser, eyeP.position, Quaternion.Euler(0, 0, a));
            SpriteRenderer srL = l.GetComponent<SpriteRenderer>();
            if (srL != null) srL.size = new Vector2(len, srL.size.y);
            Destroy(l, 1.5f);
        }
    }

    public void F_Def()
    {
        StartCoroutine(SpDef());
    }

    IEnumerator SpDef()
    {
        anim.speed = 0;
        for (int i = 0; i < 3; i++)
        {
            hp += 50f;
            if (hp > maxHp) hp = maxHp;
            float off = (i % 2 == 0) ? 0f : 15f;
            Vector2 bD = Vector2.left;

            if (t != null) bD = (t.position - handP.position).normalized;
            float bA = Mathf.Atan2(bD.y, bD.x) * Mathf.Rad2Deg;

            for (int j = -2; j <= 2; j++)
            {
                float fA = bA + (j * 20f) + off;

                RockProjectile rp = rockPool.Get();
                rp.transform.position = handP.position;
                rp.transform.rotation = Quaternion.Euler(0, 0, fA);
                Vector2 v = new Vector2(Mathf.Cos(fA * Mathf.Deg2Rad), Mathf.Sin(fA * Mathf.Deg2Rad));
                rp.Setup(v, 12f);
            }
            yield return new WaitForSeconds(0.5f);
        }
        anim.speed = 1;
    }

    public void F_Heal()
    {
        if (eyeP != null) PlaySfx(sweepLaserSound, eyeP.position);
        
        StartCoroutine(SpHeal());
    }

    IEnumerator SpHeal()
    {
        anim.speed = 0;
        float a = 90f;
        if (t != null)
        {
            Vector2 d = (t.position - eyeP.position).normalized;
            a = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + 45f;
        }

        GameObject l = Instantiate(laser, eyeP.position, Quaternion.Euler(0, 0, a));
        SpriteRenderer srL = l.GetComponent<SpriteRenderer>();

        for (int i = 0; i < 90; i++)
        {
            a -= 1f;
            hp += 3f;
            if (hp > maxHp) hp = maxHp;

            if (l != null)
            {
                l.transform.position = eyeP.position;
                l.transform.rotation = Quaternion.Euler(0, 0, a);
                Vector2 dir = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));
                Vector2 origin = (Vector2)eyeP.position + (dir * 1.2f);
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, 50f, hitMask);
                float actualLen = 50f;

                if (hit.collider != null)
                {
                    actualLen = hit.distance + 1.2f;
                    if (((1 << hit.collider.gameObject.layer) & LayerMask.GetMask("Player")) != 0)
                    {
                        PlayerBase p = hit.collider.GetComponent<PlayerBase>();
                        if (p != null) p.TakeDamage(1f, dir);
                    }
                }
                if (srL != null) srL.size = new Vector2(actualLen, srL.size.y);
            }
            yield return new WaitForSeconds(0.04f);
        }
        Destroy(l);
        anim.speed = 1;
    }
    private void PlaySfx(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            float v = 1f;
            if (AudioManager.instance != null)
            {
                v = AudioManager.instance.soundEffectsVolume;
            }
            AudioSource.PlayClipAtPoint(clip, position, v);
        }
    }
}