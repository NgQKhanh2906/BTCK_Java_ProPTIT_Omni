using UnityEngine;

public class HeadPlatform : MonoBehaviour
{
    public Collider2D hCol;
    public Collider2D pCol;

    private void Start()
    {
        if (hCol != null && pCol != null)
        {
            Physics2D.IgnoreCollision(hCol, pCol, true);
        }
    }
}