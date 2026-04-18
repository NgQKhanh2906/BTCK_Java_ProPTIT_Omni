using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private LayerMask whatIsWall; 
    
    [Header("Offset")]
    [SerializeField] private float eyeHeight = 0.5f; 

    private Transform playerTransform;

    public bool CanSeePlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, whatIsPlayer);

        if (playerCollider != null)
        {
            playerTransform = playerCollider.transform;
            Vector2 eyePos = new Vector2(transform.position.x, transform.position.y + eyeHeight);
            Vector2 targetPos = new Vector2(playerTransform.position.x, playerTransform.position.y + eyeHeight);

            RaycastHit2D hitWall = Physics2D.Linecast(eyePos, targetPos, whatIsWall);
            
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
            Vector2 eyePos = new Vector2(transform.position.x, transform.position.y + eyeHeight);
            Vector2 targetPos = new Vector2(playerTransform.position.x, playerTransform.position.y + eyeHeight);
            Gizmos.DrawLine(eyePos, targetPos);
        }
    }
}