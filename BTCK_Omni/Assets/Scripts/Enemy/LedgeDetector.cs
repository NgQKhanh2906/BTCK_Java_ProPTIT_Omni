using UnityEngine;

public class LedgeDetector : MonoBehaviour
{
    [Header("Ledge Settings")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float detectionLength = 0.5f; 
    public bool IsDetectingLedge()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, detectionLength, whatIsGround);
        return hit.collider == null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectionLength);
    }
}