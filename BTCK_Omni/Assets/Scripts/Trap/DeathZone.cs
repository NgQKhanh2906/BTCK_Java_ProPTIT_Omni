using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D c)
    {
        PlayerBase p = c.GetComponentInParent<PlayerBase>();
        
        if (p != null && !p.IsDead())
        {
            p.DieFromEnvironment();
        }
    }
}