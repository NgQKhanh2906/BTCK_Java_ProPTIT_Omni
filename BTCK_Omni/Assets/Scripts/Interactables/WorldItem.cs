using System.Collections;
using UnityEngine;
using DG.Tweening;
public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData data;
    
    private float pickupDelay = 1f;
    private float yOff = 0.2f;
    private float yTime = 1f;
    
 
    private bool _canPickup = false;
    private Rigidbody2D _rb;
    private Collider2D  _col;
 
    public bool CanInteract => _canPickup && data != null;
 
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _canPickup = true;
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
    }

    private void OnTriggerExit2D(Collider2D col)
    {
    }
    
    public void SetupAsDroppedItem(Vector2 frc)
    {
        _canPickup = false;

         if (_rb)
         {
             _rb.gravityScale = 1f;
             _rb.AddForce(frc, ForceMode2D.Impulse);
         }

        StartCoroutine(EnablePickupAfterDelay());
    }
    
 
    private IEnumerator EnablePickupAfterDelay()
    {
        yield return new WaitForSeconds(pickupDelay);
        if (_rb)
        {
            _rb.velocity = Vector2.zero;
            _rb.gravityScale = 0f;
        }
        
        transform.DOMoveY(transform.position.y + yOff, yTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        _canPickup = true;
    }
    
    public void Interact(PlayerBase player)
    {
        if (!_canPickup)
        {
            return;
        }
        TryPickup(player);
    }
    private void TryPickup(PlayerBase player)
    {
        if (!data)
        {
            return;
        }

        bool used = false;

        switch (data.itemType)
        {
            case ItemType.HealthPotion:
                if (player.CurrentHP < player.MaxHP)
                {
                    player.RestoreHP(data.healAmount);
                    used = true;
                }
                break;

            case ItemType.ManaPotion:
                if (player.CurrentMana < player.MaxMana)
                {
                    player.RestoreMana(data.manaAmount);
                    used = true;
                }
                break;
            case ItemType.ExtraLife:
                if (LivesManager.Instance)
                {
                    LivesManager.Instance.AddLife(player.playerIndex);
                    used = true;
                }
                break;
        }

        if (used)
        {
            transform.DOKill();
            Destroy(gameObject);
        }
    }
}