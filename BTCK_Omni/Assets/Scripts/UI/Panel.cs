using UnityEngine;

public class Panel : MonoBehaviour
{
    [Header("Cài đặt Panel")]
    [Tooltip("Tick: Đóng là xóa khỏi RAM. Bỏ tick: Chỉ ẩn đi, lần sau mở sẽ nhanh hơn")]
    public bool destroyOnClose = true;
    public AudioClip bgm;

    public string PanelName => name;

    public virtual void Open()
    {
        gameObject.SetActive(true);
        if (bgm != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.ChangeBGM(bgm);
        }
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        if (bgm != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeBGM();
        }

        if (destroyOnClose)
        {
            PanelManager.Instance.Unregister(PanelName);
            Destroy(gameObject);
        }
    }
}