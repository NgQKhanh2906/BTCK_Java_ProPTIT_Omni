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
}

public class SaveSystem : Singleton<SaveSystem>
{
    [SerializeField] private PlayerBase player1;
    [SerializeField] private PlayerBase player2;

    private SaveData pendingRestore;

    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public void Save()
    {
        if (player1 == null) player1 = GameObject.FindGameObjectWithTag("Player1")?.GetComponent<PlayerBase>();
        if (player2 == null) player2 = GameObject.FindGameObjectWithTag("Player2")?.GetComponent<PlayerBase>();

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            savedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
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

        if (PlayerDataManager.Instance != null && data.player1 != null)
        {
            float hp2 = data.player2 != null ? data.player2.hp : 0f;
            float mana2 = data.player2 != null ? data.player2.mana : 0f;
            PlayerDataManager.Instance.InjectFromSave(data.player1.hp, data.player1.mana, hp2, mana2);
        }

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

        if (p1 != null && pendingRestore.player1 != null) p1.RestoreState(pendingRestore.player1);
        if (p2 != null && pendingRestore.player2 != null) p2.RestoreState(pendingRestore.player2);

        if (pendingRestore.openedChestIds != null)
        {
            foreach (TreasureChest chest in FindObjectsOfType<TreasureChest>())
            {
                if (pendingRestore.openedChestIds.Contains(chest.UniqueId)) chest.RestoreState(new TreasureChest.ChestSaveData { isOpen = true });
            }
        }

        pendingRestore = null;
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
}