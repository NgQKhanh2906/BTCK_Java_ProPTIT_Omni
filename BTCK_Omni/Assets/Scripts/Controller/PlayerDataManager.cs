using System;
using UnityEngine;

public class PlayerDataManager : Singleton<PlayerDataManager>
{
    [Serializable]
    public class PlayerSnapshot
    {
        public float hp;
        public float mana;
        public bool isDead;
        public bool isCorpseLost;
        public bool hasData;
    }

    private PlayerSnapshot snap1 = new PlayerSnapshot();
    private PlayerSnapshot snap2 = new PlayerSnapshot();

    public void SaveBeforeSceneChange(PlayerBase p1, PlayerBase p2)
    {
        if (p1 != null)
        {
            snap1.hp = p1.CurrentHP;
            snap1.mana = p1.CurrentMana;
            snap1.isDead = p1.IsDead();
            snap1.isCorpseLost = p1.isCorpseLost;
            snap1.hasData = true;
        }
        if (p2 != null)
        {
            snap2.hp = p2.CurrentHP;
            snap2.mana = p2.CurrentMana;
            snap2.isDead = p2.IsDead();
            snap2.isCorpseLost = p2.isCorpseLost;
            snap2.hasData = true;
        }
    }

    public void RestorePlayerData(PlayerBase p1, PlayerBase p2)
    {
        if (p1 != null && snap1.hasData)
        {
            if (snap1.isDead)
            {
                p1.ForceDeadState(snap1.isCorpseLost);
            }
            float dHP = snap1.hp - p1.CurrentHP;
            float dMana = snap1.mana - p1.CurrentMana;
            if (Mathf.Abs(dHP) > 0.01f) p1.RestoreHP(dHP);
            if (Mathf.Abs(dMana) > 0.01f) p1.RestoreMana(dMana);
        }
        if (p2 != null && snap2.hasData)
        {
            if (snap2.isDead)
            {
                p2.ForceDeadState(snap2.isCorpseLost);
            }
            float dHP = snap2.hp - p2.CurrentHP;
            float dMana = snap2.mana - p2.CurrentMana;
            if (Mathf.Abs(dHP) > 0.01f) p2.RestoreHP(dHP);
            if (Mathf.Abs(dMana) > 0.01f) p2.RestoreMana(dMana);
        }
    }

    public void InjectFromSave(float hp1, float mana1, float hp2, float mana2)
    {
        snap1.hp = hp1; 
        snap1.mana = mana1; 
        snap1.hasData = true;
        snap2.hp = hp2; 
        snap2.mana = mana2; 
        snap2.hasData = true;
    }

    public void Clear()
    {
        snap1 = new PlayerSnapshot();
        snap2 = new PlayerSnapshot();
    }
}