using UnityEngine;

public class Panel : MonoBehaviour
{
    [Header("Cài đặt Panel")]
    [Tooltip("Tick: Đóng là xóa khỏi RAM. Bỏ tick: Chỉ ẩn đi, lần sau mở sẽ nhanh hơn")]
    public bool destroyOnClose = true;

    public AudioClip panelBGM;

    public string PanelName => name;

    public virtual void Open()
    {
        gameObject.SetActive(true);
        if (panelBGM != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(panelBGM);
        }
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        if (panelBGM != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefaultBGM();
        }

        if (destroyOnClose)
        {
            PanelManager.Instance.Unregister(PanelName);
            Destroy(gameObject);
        }
    }
}