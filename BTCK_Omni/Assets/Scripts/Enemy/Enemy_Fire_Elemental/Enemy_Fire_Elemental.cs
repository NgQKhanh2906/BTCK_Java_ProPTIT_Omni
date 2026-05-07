using UnityEngine;

public class Enemy_FireElemental : EnemyBase
{
    [Header("Fire Elemental AI Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.5f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Kamikaze Explosion")]
    [SerializeField] private GameObject explosionVfxPrefab; 
    [SerializeField] private Vector2 explosionBoxSize = new Vector2(2f, 2f); 
    [SerializeField] private float explosionDamage = 20f; 

    [Header("Audio Settings")]
    [SerializeField] private AudioClip explosionSFX; 
    [SerializeField] [Range(0f, 1f)] private float explosionVolume = 1f;
    private readonly int hashHit = Animator.StringToHash(GameConfig.ANIM_COL_HIT);

    protected override void Update()
    {
        if (isDead) return;

        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash == hashHit)
        {
            SetVelocityX(0);
            anim.SetBool(animIsMoving, false);
            return;
        }
        Collider2D playerInExplosionRange = Physics2D.OverlapBox(transform.position, explosionBoxSize, 0f, targetLayer);
        if (playerInExplosionRange != null)
        {
            Explode();
            return; 
        }
        Transform target = GetVisiblePlayer();
        if (target != null)
        {
            isReturningHome = false;
            ChasePlayer(target);
            return;
        }
        if (isReturningHome)
        {
            ReturnHomeLogic();
            return;
        }

        float distToHome = Mathf.Abs(transform.position.x - startPosition.x);
        if (distToHome > maxWanderDistance && !isIdle)
        {
            isReturningHome = true;
        }

        if (isReturningHome)
        {
            ReturnHomeLogic();
            return;
        }

        base.Update();
    }

    protected override void ChasePlayer(Transform target)
    {
        bool isLedgeAhead = ledgeCheck != null && ledgeCheck.IsDetectingLedge();
        bool isWallAhead = IsWallDetected();

        if (isWallAhead || isLedgeAhead)
        {
            SetVelocityX(0); 
            anim.SetBool(animIsMoving, false); 
            
            float dirToPlayerX = target.position.x - transform.position.x;
            if ((dirToPlayerX > 0 && facingDir == -1) || (dirToPlayerX < 0 && facingDir == 1))
            {
                Flip(); 
            }
        }
        else 
        {
            isIdle = false; 
            anim.SetBool(animIsMoving, true);
            
            float dirToPlayerX = target.position.x - transform.position.x;
            int moveDir = dirToPlayerX > 0 ? 1 : -1;
            
            if (moveDir != facingDir) Flip();
            
            SetVelocityX(moveSpeed * chaseSpeedMultiplier * facingDir);
        }
    }

    private void Explode()
    {
        isDead = true; 
        SetVelocityX(0);
        if (explosionSFX != null)
        {
            AudioSource.PlayClipAtPoint(explosionSFX, transform.position, explosionVolume);
        }
        if (explosionVfxPrefab != null)
        {
            Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
        }
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, explosionBoxSize, 0f, targetLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable target))
            {
                Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                if (knockDir.y <= 0) knockDir.y = 0.5f; 
                
                target.TakeDamage(explosionDamage, knockDir);
            }
        }
        Destroy(gameObject);
    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, explosionBoxSize);
    }
}