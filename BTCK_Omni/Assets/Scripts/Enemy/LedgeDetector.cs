using UnityEngine;

public class LedgeDetector : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;

    private void Update()
    {
        // Bắn tia laser ngắn xuống đất để xem có vực không
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, whatIsGround);
        isGrounded = hit.collider != null;
    }

    public bool IsDetectingLedge()
    {
        // Trả về TRUE nếu phía trước LÀ VỰC (tức là không chạm đất)
        return !isGrounded;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);
    }
}