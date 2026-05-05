using UnityEngine;

public class LedgeDetector : MonoBehaviour
{
    [Header("Ledge Detection Settings")]
    [SerializeField] private float raycastDistance = 1.5f;
    [Tooltip("Layer của mặt đất (Thường là Ground)")]
    [SerializeField] private LayerMask whatIsGround;
    public bool IsDetectingLedge()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, whatIsGround);
        return hit.collider == null;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 endPosition = transform.position + Vector3.down * raycastDistance;
        Gizmos.DrawLine(transform.position, endPosition);
    }
}