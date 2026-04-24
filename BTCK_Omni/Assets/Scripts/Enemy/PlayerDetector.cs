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
        // 1. Quét vòng tròn xem có Player trong vùng không
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, whatIsPlayer);

        if (playerCollider != null)
        {
            playerTransform = playerCollider.transform;
            
            // Điểm bắt đầu: Mắt sói
            Vector2 eyePos = new Vector2(transform.position.x, transform.position.y + eyeHeight);
            
            // ĐÃ SỬA: Điểm đích: Chính giữa cơ thể của Player (chính xác hơn việc tự cộng trừ Y)
            Vector2 targetPos = playerCollider.bounds.center;

            // 2. Bắn tia nhìn Line of Sight
            RaycastHit2D hitWall = Physics2D.Linecast(eyePos, targetPos, whatIsWall);
            
            // 3. Nếu tia không đụng trúng tường/sàn -> Nhìn thấy! (Dù nhảy lên trời cũng vẫn thấy)
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
            
            // Vẽ tia Linecast để em dễ debug trong Editor
            // Nếu Player có collider, dùng tâm collider. Nếu chưa lấy được thì cộng eyeHeight tạm
            Vector2 targetPos = playerTransform.GetComponent<Collider2D>() != null 
                                ? playerTransform.GetComponent<Collider2D>().bounds.center 
                                : new Vector2(playerTransform.position.x, playerTransform.position.y + eyeHeight);
            
            Gizmos.DrawLine(eyePos, targetPos);
        }
    }
}