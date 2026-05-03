using UnityEngine;

public enum ItemType
{
    HealthPotion, ManaPotion, ExtraLife
}
[CreateAssetMenu(fileName = "Data_NewItem", menuName = "Game Data/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Thông tin")]
    public string itemName;
    public ItemType itemType;
    public Sprite icon;       
 
    [Header("Giá trị hồi")]
    public float healAmount;
    public float manaAmount;
 
    [Header("Prefab trong world")]
    public GameObject worldPrefab;
}