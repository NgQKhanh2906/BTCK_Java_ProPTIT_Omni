using UnityEngine;

public class IntersectionTransition : MonoBehaviour
{
    [Header("Kéo các Room xung quanh ngã 3/ngã 4 vào đây")]
    [Tooltip("Hướng nào không có Room thì cứ để trống (None)")]
    public GameObject phongTrai;
    public GameObject phongPhai;
    public GameObject phongTren;
    public GameObject phongDuoi;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (phongTrai != null) phongTrai.SetActive(true);
            if (phongPhai != null) phongPhai.SetActive(true);
            if (phongTren != null) phongTren.SetActive(true);
            if (phongDuoi != null) phongDuoi.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Vector2 huongThoat = other.transform.position - transform.position;
            
            if (Mathf.Abs(huongThoat.x) > Mathf.Abs(huongThoat.y))
            {
                if (huongThoat.x > 0) 
                {
                    TatCacPhongNgoaiTru(phongPhai);
                }
                else 
                {
                    TatCacPhongNgoaiTru(phongTrai);
                }
            }
            else
            {
                if (huongThoat.y > 0)
                {
                    TatCacPhongNgoaiTru(phongTren);
                }
                else
                {
                    TatCacPhongNgoaiTru(phongDuoi);
                }
            }
        }
    }
    
    private void TatCacPhongNgoaiTru(GameObject phongGiuLai)
    {
        if (phongGiuLai != null) phongGiuLai.SetActive(true);
        if (phongTrai != null && phongTrai != phongGiuLai) phongTrai.SetActive(false);
        if (phongPhai != null && phongPhai != phongGiuLai) phongPhai.SetActive(false);
        if (phongTren != null && phongTren != phongGiuLai) phongTren.SetActive(false);
        if (phongDuoi != null && phongDuoi != phongGiuLai) phongDuoi.SetActive(false);
    }
}