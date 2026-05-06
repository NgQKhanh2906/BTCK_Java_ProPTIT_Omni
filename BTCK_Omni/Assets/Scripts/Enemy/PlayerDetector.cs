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
        if (playerTransform != null)
        {
            float distToCurrent = Vector2.Distance(transform.position, playerTransform.position);
            if (distToCurrent <= detectionRadius)
            {
                RaycastHit2D hitWall = Physics2D.Linecast(transform.position, playerTransform.position, whatIsWall);
                if (hitWall.collider == null)
                {
                    return true; 
                }
            }
            playerTransform = null; 
        }
        Collider2D[] playersInRange = Physics2D.OverlapCircleAll(transform.position, detectionRadius, whatIsPlayer);
        
        Transform closestPlayer = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D playerCollider in playersInRange)
        {
            RaycastHit2D hitWall = Physics2D.Linecast(transform.position, playerCollider.transform.position, whatIsWall);
            
            if (hitWall.collider == null) 
            {
                float dist = Vector2.Distance(transform.position, playerCollider.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPlayer = playerCollider.transform;
                }
            }
        }
        if (closestPlayer != null)
        {
            playerTransform = closestPlayer;
            return true;
        }

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