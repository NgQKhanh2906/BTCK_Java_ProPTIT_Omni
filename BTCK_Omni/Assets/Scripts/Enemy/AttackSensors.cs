using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackSensor : MonoBehaviour
{
    private Enemy_Ghoul parentGhoul;
    private void Awake()
    {
        parentGhoul = GetComponentInParent<Enemy_Ghoul>();
        GetComponent<Collider2D>().isTrigger = true; 
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (parentGhoul != null) parentGhoul.SetPlayerInAttackRange(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (parentGhoul != null) parentGhoul.SetPlayerInAttackRange(false);
        }
    }
}