using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = System.Random;

[Serializable]
public class LootDrop
{
    public ItemData item;
    public int dropWeight; 
}

public class TreasureChest : MonoBehaviour, IInteractable, ISaveable
{
    [SerializeField] private string chestId;
 
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite openSprite;
    
    [Header("Loot")]
    [SerializeField] private List<LootDrop> lootTable;
    [SerializeField] private int minItemCount = 1;
    [SerializeField] private int maxItemCount = 5;
 
    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;
 
    [Header("Force")]
    [SerializeField] private float launchForceY = 5f;
    [SerializeField] private float spreadX = 2f;
    
    private bool _isOpen    = false;
    private bool _isOpening = false;
 
    public string UniqueId => chestId;
    public bool CanInteract => !_isOpen && !_isOpening;
    
    
    public void Interact(PlayerBase player)
    {
        if (!CanInteract) return;
        StartCoroutine(OpenSequence());
    }
    private IEnumerator OpenSequence()
    {
        _isOpening = true;
        
        transform.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.1f).SetLoops(2, LoopType.Yoyo);
        yield return new WaitForSeconds(0.15f);

        
        if (spriteRenderer && openSprite)
        {
            spriteRenderer.sprite = openSprite;
        }
        transform.DOShakePosition(0.2f, 0.1f, 10, 90f, false, true);
        
        SpawnItems();

        _isOpen = true;
        _isOpening= false;
        //SaveManager.Instance?.RegisterOpenedChest(chestId);
    }
    public void SpawnItems()
    {
        if (lootTable == null || lootTable.Count == 0)
        {
            return;
        }
        
        int dropCount = UnityEngine.Random.Range(minItemCount, maxItemCount + 1);
        int totalWeight = 0;
        foreach (var loot in lootTable)
        {
            totalWeight += loot.dropWeight;
        }
        
        Vector2 origin = (spawnPoint) ? (Vector2)spawnPoint.position : (Vector2)transform.position + Vector2.up * 0.5f;
        for (int i = 0; i < dropCount; i++)
        {
            int randomRoll = UnityEngine.Random.Range(0, totalWeight);
            ItemData pickedItem = null;
            int currentWeight = 0;
            foreach (var loot in lootTable)
            {
                currentWeight += loot.dropWeight;
                if (randomRoll < currentWeight)
                {
                    pickedItem = loot.item;
                    break;
                }
            }

            if (!pickedItem|| !pickedItem.worldPrefab) continue;
            GameObject obj = Instantiate(pickedItem.worldPrefab, origin, Quaternion.identity);
            int dir = UnityEngine.Random.value > 0.5f ? 1 : -1;
            float rx = UnityEngine.Random.Range(1, spreadX);
            rx *= dir;
            var force = new Vector2(rx, launchForceY);
            var item = obj.GetComponent<WorldItem>();
            if (item)
            {
                item.SetupAsDroppedItem(force);
            }
        }
    }
    
    
    [Serializable]
    public class ChestSaveData { public bool isOpen; }
 
    public object CaptureState() => new ChestSaveData { isOpen = _isOpen };
 
    public void RestoreState(object state)
    {
        if (state is not ChestSaveData s || !s.isOpen)
        {
            return;
        }
        _isOpen = true;
        _isOpening = false;
        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }
    }
}