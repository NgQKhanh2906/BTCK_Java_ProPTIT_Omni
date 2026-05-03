using System.Collections.Generic;
using UnityEngine;

public class IntersectionTransition : MonoBehaviour
{
    [Header("Kéo các Room xung quanh ngã 3/ngã 4 vào đây")]
    [Tooltip("Hướng nào không có Room thì cứ để trống (None)")]
    public GameObject phongTrai; 
    public GameObject phongPhai;
    public GameObject phongTren;
    public GameObject phongDuoi;

    private Collider2D myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Player1") || other.CompareTag("Player2")))
        {
            if (phongTrai != null) phongTrai.SetActive(true);
            if (phongPhai != null) phongPhai.SetActive(true);
            if (phongTren != null) phongTren.SetActive(true);
            if (phongDuoi != null) phongDuoi.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if ((other.CompareTag("Player1") || other.CompareTag("Player2")))
        {
            KiemTraVaTatMap();
        }
    }
    
    private void KiemTraVaTatMap()
    {
        List<GameObject> tatCaNguoiChoi = new List<GameObject>();
        tatCaNguoiChoi.AddRange(GameObject.FindGameObjectsWithTag("Player1"));
        tatCaNguoiChoi.AddRange(GameObject.FindGameObjectsWithTag("Player2"));
        
        foreach (GameObject p in tatCaNguoiChoi)
        {
            if (myCollider.bounds.Contains(p.transform.position))
            {
                return; 
            }
        }
        
        bool giuTrai = false, giuPhai = false, giuTren = false, giuDuoi = false;

        foreach (GameObject p in tatCaNguoiChoi)
        {
            Vector2 huong = p.transform.position - myCollider.bounds.center;
            
            if (Mathf.Abs(huong.x) > Mathf.Abs(huong.y))
            {
                if (huong.x < 0) giuTrai = true;  
                else giuPhai = true;
            }
            else
            {
                if (huong.y < 0) giuDuoi = true;
                else giuTren = true;
            }
        }
        
        if (phongTrai != null) phongTrai.SetActive(giuTrai);
        if (phongPhai != null) phongPhai.SetActive(giuPhai);
        if (phongTren != null) phongTren.SetActive(giuTren);
        if (phongDuoi != null) phongDuoi.SetActive(giuDuoi);
    }
}