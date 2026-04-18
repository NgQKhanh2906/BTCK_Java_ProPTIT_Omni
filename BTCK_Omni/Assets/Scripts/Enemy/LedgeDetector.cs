<<<<<<< HEAD
using UnityEngine;

public class LedgeDetector : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, whatIsGround);
        isGrounded = hit.collider != null;
    }

    public bool IsDetectingLedge()
    {
        return !isGrounded;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);
    }
}
=======
using UnityEngine;

public class LedgeDetector : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, whatIsGround);
        isGrounded = hit.collider != null;
    }

    public bool IsDetectingLedge()
    {
        return !isGrounded;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);
    }
}
>>>>>>> f3cfb5b531d959a1f8f4f22572497dccfc69110e
