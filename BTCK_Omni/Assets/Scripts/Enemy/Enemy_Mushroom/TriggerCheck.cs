using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    public bool isAttackArea; 
    private Enemy_Mushroom mushroom;
    void Awake() => mushroom = GetComponentInParent<Enemy_Mushroom>();
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isAttackArea) mushroom.SetPlayerAttack(true);
            else mushroom.SetPlayerDetection(true, collision.transform);
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isAttackArea) mushroom.SetPlayerAttack(false);
            else mushroom.SetPlayerDetection(false, null);
        }
    }
}