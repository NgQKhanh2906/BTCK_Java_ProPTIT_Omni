using UnityEngine;
using System.Collections;

public class CoopDoorSystem : MonoBehaviour
{
    [Header("Kéo 2 cái cửa vào đây")]
    public DoorSensor cuaSo1;
    public DoorSensor cuaSo2;

    [Header("Cấu hình Room")]
    public GameObject roomHienTai;
    public GameObject roomTiepTheo;
    public Transform diemSpawnMoi;

    [Header("Hiệu ứng")]
    public CanvasGroup manHinhDen;
    public float thoiGianToi = 0.5f;

    private bool dangChuyenCanh = false;
    
    private void Update()
    {
        if (!dangChuyenCanh && cuaSo1.coNguoi && cuaSo2.coNguoi)
        {
            StartCoroutine(ChuyenCanhCoop());
        }
    }

    private IEnumerator ChuyenCanhCoop()
    {
        dangChuyenCanh = true;
        
        GameObject[] tatCaNguoiChoi = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in tatCaNguoiChoi)
        {
            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        float timer = 0;
        while (timer < thoiGianToi)
        {
            timer += Time.deltaTime;
            manHinhDen.alpha = timer / thoiGianToi;
            yield return null;
        }
        manHinhDen.alpha = 1;
        
        if (roomHienTai != null) roomHienTai.SetActive(false);
        if (roomTiepTheo != null) roomTiepTheo.SetActive(true);
        
        if (tatCaNguoiChoi.Length >= 2)
        {
            tatCaNguoiChoi[0].transform.position = diemSpawnMoi.position; 
            tatCaNguoiChoi[1].transform.position = diemSpawnMoi.position + new Vector3(1.5f, 0, 0); 
        }

        yield return new WaitForSeconds(0.2f);
        
        timer = 0;
        while (timer < thoiGianToi)
        {
            timer += Time.deltaTime;
            manHinhDen.alpha = 1 - (timer / thoiGianToi);
            yield return null;
        }
        manHinhDen.alpha = 0;

        foreach (GameObject p in tatCaNguoiChoi)
        {
            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
            if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        dangChuyenCanh = false;
    }
}