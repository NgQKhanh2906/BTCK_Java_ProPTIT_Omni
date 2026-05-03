using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData
{
    public string sceneName;
    public string savedAt;
    public int lives1;
    public int lives2;
    public PlayerBase.PlayerSaveState player1;
    public PlayerBase.PlayerSaveState player2;
    public List<string> openedChestIds = new List<string>();
    public int roomIdx;
}

public class SaveSystem : Singleton<SaveSystem>
{
    private PlayerBase player1;
    private PlayerBase player2;
    private SaveData pendingRestore;

    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public void Save()
    {
        PlayerBase[] allPlayers = FindObjectsOfType<PlayerBase>();
        foreach (PlayerBase p in allPlayers)
        {
            if (p.playerIndex == 1) player1 = p;
            if (p.playerIndex == 2) player2 = p;
        }

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            savedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            roomIdx = MapManager.Instance != null ? MapManager.Instance.GetCurIdx() : 0,
            lives1 = LivesManager.Instance != null ? LivesManager.Instance.GetLives(1) : 0,
            lives2 = LivesManager.Instance != null ? LivesManager.Instance.GetLives(2) : 0,
            openedChestIds = new List<string>()
        };

        if (player1 != null) data.player1 = (PlayerBase.PlayerSaveState)player1.CaptureState();
        if (player2 != null) data.player2 = (PlayerBase.PlayerSaveState)player2.CaptureState();

        foreach (TreasureChest chest in FindObjectsOfType<TreasureChest>())
        {
            TreasureChest.ChestSaveData state = chest.CaptureState() as TreasureChest.ChestSaveData;
            if (state != null && state.isOpen) data.openedChestIds.Add(chest.UniqueId);
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public void Load()
    {
        if (!HasSave()) return;
        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.SetLivesDirectly(1, data.lives1);
            LivesManager.Instance.SetLivesDirectly(2, data.lives2);
        }

        pendingRestore = data;
        SceneManager.LoadScene(data.sceneName);
    }

    public void RestoreAfterLoad(PlayerBase p1, PlayerBase p2)
    {
        if (pendingRestore == null) return;
        if (MapManager.Instance != null)
        {
            MapManager.Instance.SetRoom(pendingRestore.roomIdx);
        }
        bool daNhanThongTin = false;

        if (p1 != null && pendingRestore.player1 != null)
        {
            p1.RestoreState(pendingRestore.player1);
            pendingRestore.player1 = null;
            daNhanThongTin = true;
        }

        if (p2 != null && pendingRestore.player2 != null)
        {
            p2.RestoreState(pendingRestore.player2);
            pendingRestore.player2 = null;
            daNhanThongTin = true;
        }

        if (daNhanThongTin && pendingRestore.openedChestIds != null)
        {
            foreach (TreasureChest chest in FindObjectsOfType<TreasureChest>())
            {
                if (pendingRestore.openedChestIds.Contains(chest.UniqueId))
                {
                    chest.RestoreState(new TreasureChest.ChestSaveData { isOpen = true });
                }
            }
            pendingRestore.openedChestIds = null;
        }

        if (pendingRestore.player1 == null && pendingRestore.player2 == null)
        {
            pendingRestore = null;
        }
    }

    public bool HasSave() => File.Exists(SavePath);

    public string GetSaveTime()
    {
        if (!HasSave()) return "";
        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        return data != null ? data.savedAt : "";
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
    public bool IsLoading() => pendingRestore != null;
    public int GetPendingRoomIdx()
    {
        return pendingRestore != null ? pendingRestore.roomIdx : 0;
    }
}