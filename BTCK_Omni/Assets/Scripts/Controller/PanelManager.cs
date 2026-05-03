using System.Collections.Generic;
using UnityEngine;

public class PanelManager : Singleton<PanelManager>
{
    [Header("Kho chứa UI Prefabs (Kéo thả Prefab vào đây)")]
    public List<Panel> panelPrefabs; 
    private Dictionary<string, Panel> activePanels = new Dictionary<string, Panel>();

    public Panel GetPanel(string panelName)
    {
        if (IsExisted(panelName))
        {
            return activePanels[panelName];
        }
        Panel prefabToLoad = null;
        foreach (var prefab in panelPrefabs)
        {
            if (prefab.gameObject.name == panelName)
            {
                prefabToLoad = prefab;
                break;
            }
        }
        
        if (prefabToLoad == null)
        {
            Debug.LogError("Lỗi PanelManager: Không tìm thấy Prefab nào tên là [" + panelName + "] trong danh sách!");
            return null; 
        }
        
        Panel newPanel = Instantiate(prefabToLoad, this.transform);
        newPanel.name = panelName; 
        newPanel.gameObject.SetActive(false);

        activePanels[panelName] = newPanel;
        return newPanel;
    }

    public void OpenPanel(string name)
    {
        Panel panel = GetPanel(name);
        if (panel != null) panel.Open();
    }

    public void ClosePanel(string name)
    {
        Panel panel = GetPanel(name);
        if (panel != null) panel.Close();
    }

    public void CloseAllPanels()
    {
        List<string> keysToClose = new List<string>(activePanels.Keys); 
        foreach (string key in keysToClose)
        {
            ClosePanel(key); 
        }
    }

    public bool IsExisted(string name)
    {
        return activePanels.ContainsKey(name);
    }

    public void Unregister(string name)
    {
        activePanels.Remove(name);
    }
}