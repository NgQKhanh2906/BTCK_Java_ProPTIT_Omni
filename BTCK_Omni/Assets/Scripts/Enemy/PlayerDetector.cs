using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private LayerMask whatIsWall; 

    private Transform playerTransform;
    public bool CanSeePlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, whatIsPlayer);
        if (playerCollider != null)
        {
            playerTransform = playerCollider.transform;
            RaycastHit2D hitWall = Physics2D.Linecast(transform.position, playerTransform.position, whatIsWall);
            if (hitWall.collider == null)
            {
                return true; 
            }
        }
        playerTransform = null;
        return false;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}