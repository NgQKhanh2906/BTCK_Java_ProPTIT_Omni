using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    
    [SerializeField] private float groundDist = 0.5f;
    [SerializeField] private float slopeDist = 0.8f;
    [SerializeField] private float maxSlope = 45f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    
    public bool IsGrounded { get; private set; }
    public bool IsOnSlope { get; private set; }
    
    public Vector2 SlopeNormal { get; private set; } = Vector2.up;
    //public float SlopeAngle { get; private set; }

    public void Check(bool isJumping)
    {
        if (isJumping)
        {
            IsGrounded = false;
            IsOnSlope = false;
            SlopeNormal = Vector2.up;
            return;
        }
        Vector2 posL = leftFoot.position;
        Vector2 posR = rightFoot.position;
        RaycastHit2D hitL = Physics2D.Raycast(posL, Vector2.down, slopeDist, groundLayer);
        RaycastHit2D hitR = Physics2D.Raycast(posR, Vector2.down, slopeDist, groundLayer);

        bool isGroundLeft = false, isOnSlopeLeft = false;
        bool isGroundRight = false, isOnSlopeRight = false; 
        Vector2 normL = Vector2.up, normR = Vector2.up;
        if (hitL)
        {
            normL = hitL.normal;
            float angle = Vector2.Angle(normL, Vector2.up);
            isOnSlopeLeft = (angle > 0.1f&& angle <= maxSlope);

            float currentDist = (isOnSlopeLeft && !isJumping) ? slopeDist : groundDist;
            isGroundLeft = hitL.distance <= currentDist;

            if (!isGroundLeft)
            {
                isOnSlopeLeft = false;
            }
        }
        if (hitR)
        {
            normR = hitR.normal;
            float angle = Vector2.Angle(normR, Vector2.up);
            isOnSlopeRight = (angle > 0.1f&& angle <= maxSlope);

            float currentDist = (isOnSlopeRight && !isJumping) ? slopeDist : groundDist;
            isGroundRight = hitR.distance <= currentDist;

            if (!isGroundRight)
            {
                isOnSlopeRight = false;
            }
        }
        IsGrounded = isGroundLeft || isGroundRight;
        IsOnSlope = isOnSlopeLeft || isOnSlopeRight;
        if (isOnSlopeLeft) SlopeNormal = normL;
        else if(isOnSlopeRight) SlopeNormal = normR;
        else SlopeNormal = Vector2.up;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        if (leftFoot) DrawFootGizmo(leftFoot.position);
        if (rightFoot) DrawFootGizmo(rightFoot.position);
    }
    
    private void DrawFootGizmo(Vector2 pos)
    {
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector2.down * groundDist);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawLine(pos + Vector2.down * groundDist, pos + Vector2.down * slopeDist);
    }
}