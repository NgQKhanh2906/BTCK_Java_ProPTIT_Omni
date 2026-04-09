using UnityEngine;

public class WolfTriggerArea : MonoBehaviour
{
    public bool isAttackArea; 
    private Enemy_Wolf wolf;

    void Awake() => wolf = GetComponentInParent<Enemy_Wolf>();

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isAttackArea) wolf.SetPlayerAttack(true);
            else wolf.SetPlayerDetection(true, collision.transform);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isAttackArea) wolf.SetPlayerAttack(false);
            else wolf.SetPlayerDetection(false, null);
        }
    }
}