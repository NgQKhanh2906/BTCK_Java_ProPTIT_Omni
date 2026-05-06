using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public Transform pt;
    public float rad = 1f;
    public LayerMask mask;
    public GameObject icon;
    public KeyCode key;

    private PlayerBase p;
    private IInteractable t;

    private void Awake()
    {
        p = GetComponent<PlayerBase>();
        icon.SetActive(false);
    }

    private void Update()
    {
        if (p != null && p.IsDead())
        {
            if (icon != null && icon.activeSelf) icon.SetActive(false);
            return;
        }
        
        FindObj();

        if (t != null && Input.GetKeyDown(key))
        {
            t.Interact(p);
        }
    }

    private void FindObj()
    {
        Collider2D[] arr = Physics2D.OverlapCircleAll(pt.position, rad, mask);
        t = null;
        float min = Mathf.Infinity;

        foreach (Collider2D c in arr)
        {
            IInteractable obj = c.GetComponent<IInteractable>();
            if (obj != null && obj.CanInteract)
            {
                float d = Vector2.Distance(transform.position, c.transform.position);
                if (d < min)
                {
                    min = d;
                    t = obj;
                }
            }
        }

        icon.SetActive(t != null);
    }
}