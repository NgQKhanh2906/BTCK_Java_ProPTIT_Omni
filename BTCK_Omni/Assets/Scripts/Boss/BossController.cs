using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Đổi từ MonoBehaviour sang Entity để nhận Dame từ Arrow và hệ thống vũ khí của bạn
public class BossController : Entity
{
    public GameObject rock;
    public GameObject laser;
    public Transform eyeP;
    public Transform handP;

    [Header("Boss Stats")]
    public float hp = 3000f;
    public float maxHp = 3000f;
    public LayerMask hitMask;

    [Header("Facing Direction")]
    // Tick vào ô này ở Inspector nếu ảnh gốc của con Boss đang nhìn sang phải.
    // Nếu ảnh gốc nhìn sang trái thì BỎ TICK.
    public bool isFacingRight = false;

    Transform t;

    protected override void Awake()
    {
        base.Awake(); // Gọi base.Awake() của Entity nếu có
    }

    void Start()
    {
        StartCoroutine(Loop());
    }

    void Update()
    {
        if (isDead) return; // Nếu Boss chết thì không tìm hay quay mặt nữa

        Find();
        LookAtPlayer();
    }

    void Find()
    {
        int pL = LayerMask.GetMask("Player");
        Collider2D[] ps = Physics2D.OverlapCircleAll(transform.position, 50f, pL);
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

    // --- LOGIC QUAY MẶT VỀ PHÍA PLAYER ---
    void LookAtPlayer()
    {
        if (t != null)
        {
            // Nếu Player ở bên phải Boss nhưng Boss đang nhìn trái -> Lật
            if (transform.position.x > t.position.x && !isFacingRight)
            {
                FlipBoss();
            }
            // Nếu Player ở bên trái Boss nhưng Boss đang nhìn phải -> Lật
            else if (transform.position.x < t.position.x && isFacingRight)
            {
                FlipBoss();
            }
        }
    }

    void FlipBoss() // Đổi tên ở đây
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // --- LOGIC NHẬN SÁT THƯƠNG TỪ PLAYER ---
    public override void TakeDamage(float dmg, Vector2 hitDir)
    {
        if (isDead) return;

        // Nếu Entity của bạn có logic giật lùi hoặc nhấp nháy đỏ khi trúng đòn, 
        // bạn có thể giữ nguyên dòng base.TakeDamage dưới đây.
        base.TakeDamage(dmg, hitDir);

        hp -= dmg;

        // Kích hoạt animation bị thương của Boss nếu có
        // anim.SetTrigger("takehit"); 

        if (hp <= 0)
        {
            Die();
        }
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
        SceneManager.LoadScene("Menu");
    }

    // ==========================================
    // CÁC CHIÊU THỨC GIỮ NGUYÊN HOẠT ĐỘNG
    // ==========================================

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
            GameObject r = Instantiate(rock, handP.position, Quaternion.Euler(0, 0, a));
            RockProjectile rp = r.GetComponent<RockProjectile>();
            if (rp != null) rp.Setup(d, 15f);
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
        for (int i = 0; i < 10; i++)
        {
            float rx = transform.position.x + Random.Range(-15f, 15f);
            Vector2 rayStart = new Vector2(rx, transform.position.y + 20f);
            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 40f, gl);
            if (hit.collider != null)
            {
                Vector3 p = new Vector3(rx, hit.point.y + 0.2f, 0);
                GameObject g = Instantiate(rock, p, Quaternion.Euler(0, 0, 90f));
                RockProjectile rp = g.GetComponent<RockProjectile>();
                if (rp != null) rp.Setup(Vector2.up, 15f);
            }
            yield return new WaitForSeconds(0.15f);
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
            SpriteRenderer sr = l.GetComponent<SpriteRenderer>();
            if (sr != null) sr.size = new Vector2(len, sr.size.y);
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
                GameObject r = Instantiate(rock, handP.position, Quaternion.Euler(0, 0, fA));
                RockProjectile rp = r.GetComponent<RockProjectile>();
                if (rp != null)
                {
                    Vector2 v = new Vector2(Mathf.Cos(fA * Mathf.Deg2Rad), Mathf.Sin(fA * Mathf.Deg2Rad));
                    rp.Setup(v, 12f);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
        anim.speed = 1;
    }

    public void F_Heal()
    {
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
        SpriteRenderer sr = l.GetComponent<SpriteRenderer>();

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
                if (sr != null) sr.size = new Vector2(actualLen, sr.size.y);
            }
            yield return new WaitForSeconds(0.02f);
        }
        Destroy(l);
        anim.speed = 1;
    }
}