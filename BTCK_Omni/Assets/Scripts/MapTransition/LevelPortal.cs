using UnityEngine;
using System.Collections.Generic;

public class OmniLevelPortal : MonoBehaviour
{
    [Tooltip("Tên của Map tiếp theo. Ví dụ: Map3")]
    public string nextSceneName;

    private List<Collider2D> playersInZone = new List<Collider2D>();
    private bool dangChuyenMap = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playersInZone.Contains(other))
        {
            playersInZone.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playersInZone.Contains(other))
        {
            playersInZone.Remove(other);
        }
    }

    private void Update()
    {
        if (dangChuyenMap) return;

        if (playersInZone.Count > 0)
        {
            CheckAndLoadScene();
        }
    }

    private void CheckAndLoadScene()
    {
        if (CamController.Instance == null) return;
        int soNguoiConSong = CamController.Instance.players.Count;
        
        if (playersInZone.Count >= soNguoiConSong && soNguoiConSong > 0)
        {
            dangChuyenMap = true; 
            playersInZone.Clear(); 
            if (GlobalFader.Instance != null)
            {
                GlobalFader.Instance.ChuyenMap(nextSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}