using UnityEngine;

public class LedgeDetector : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGround;
    public bool IsDetectingLedge()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, whatIsGround);
        return hit.collider == null; 
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);
    }
}