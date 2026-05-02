using System;
using UnityEngine;

public class PlayerDataManager : Singleton<PlayerDataManager>
{
    [Serializable]
    public class PlayerSnapshot
    {
        public float hp;
        public float mana;
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
            snap1.hasData = true;
        }
        if (p2 != null)
        {
            snap2.hp = p2.CurrentHP;
            snap2.mana = p2.CurrentMana;
            snap2.hasData = true;
        }
    }

    public void RestorePlayerData(PlayerBase p1, PlayerBase p2)
    {
        if (p1 != null && snap1.hasData)
        {
            float hpDelta = snap1.hp - p1.CurrentHP;
            float manaDelta = snap1.mana - p1.CurrentMana;
            if (Mathf.Abs(hpDelta) > 0.01f) p1.RestoreHP(hpDelta);
            if (Mathf.Abs(manaDelta) > 0.01f) p1.RestoreMana(manaDelta);
        }
        if (p2 != null && snap2.hasData)
        {
            float hpDelta = snap2.hp - p2.CurrentHP;
            float manaDelta = snap2.mana - p2.CurrentMana;
            if (Mathf.Abs(hpDelta) > 0.01f) p2.RestoreHP(hpDelta);
            if (Mathf.Abs(manaDelta) > 0.01f) p2.RestoreMana(manaDelta);
        }
    }

    public void InjectFromSave(float hp1, float mana1, float hp2, float mana2)
    {
        snap1.hp = hp1; snap1.mana = mana1; snap1.hasData = true;
        snap2.hp = hp2; snap2.mana = mana2; snap2.hasData = true;
    }

    public void Clear()
    {
        snap1 = new PlayerSnapshot();
        snap2 = new PlayerSnapshot();
    }
}