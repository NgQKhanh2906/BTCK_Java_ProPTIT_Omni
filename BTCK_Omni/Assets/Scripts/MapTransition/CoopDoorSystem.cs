using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoopDoorSystem : MonoBehaviour
{
    [Header("Kéo 2 cái cửa vào đây")]
    public DoorSensor cuaSo1;
    public DoorSensor cuaSo2;

    [Header("Cấu hình Room & Camera")]
    public GameObject roomHienTai;
    public GameObject roomTiepTheo;
    public Transform diemSpawnMoi;
    [Tooltip("Kéo Camera Limit của Room tiếp theo vào đây")]
    public Collider2D newCameraLimit; 

    [Header("Hiệu ứng")]
    public CanvasGroup manHinhDen;
    public float thoiGianToi = 0.5f;

    private bool dangChuyenCanh = false;
    
    private void Update()
    {
        if (dangChuyenCanh) return;
        int soNguoiConSong = (CamController.Instance != null) ? CamController.Instance.players.Count : 2;

        bool duDieuKienQuaCua = false;
        
        if (soNguoiConSong >= 2)
        {
            duDieuKienQuaCua = cuaSo1.coNguoi && cuaSo2.coNguoi;
        }
        else if (soNguoiConSong == 1)
        {
            duDieuKienQuaCua = cuaSo1.coNguoi || cuaSo2.coNguoi;
        }

        if (duDieuKienQuaCua)
        {
            StartCoroutine(ChuyenCanhCoop());
        }
    }

    private IEnumerator ChuyenCanhCoop()
    {
        dangChuyenCanh = true;
        
        List<GameObject> nguoiChoiHienTai = new List<GameObject>();
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.activeInHierarchy) nguoiChoiHienTai.Add(p);
        }
        
        foreach (GameObject p in nguoiChoiHienTai)
        {
            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        
        float timer = 0;
        if (manHinhDen != null)
        {
            while (timer < thoiGianToi)
            {
                timer += Time.deltaTime;
                manHinhDen.alpha = timer / thoiGianToi;
                yield return null;
            }
            manHinhDen.alpha = 1;
        }
        
        
        if (roomHienTai != null) roomHienTai.SetActive(false);
        if (roomTiepTheo != null) roomTiepTheo.SetActive(true);
        
        for (int i = 0; i < nguoiChoiHienTai.Count; i++)
        {
            nguoiChoiHienTai[i].transform.position = diemSpawnMoi.position + new Vector3(i * 2.0f, 0, 0);
        }
        
        if (CamController.Instance != null && newCameraLimit != null)
        {
            CamController.Instance.ChangeLimit(newCameraLimit);
            CamController.Instance.SnapCamera(newCameraLimit.bounds.center);
        }
        
        yield return new WaitForSeconds(0.2f); 
        
        timer = 0;
        if (manHinhDen != null)
        {
            while (timer < thoiGianToi)
            {
                timer += Time.deltaTime;
                manHinhDen.alpha = 1 - (timer / thoiGianToi);
                yield return null;
            }
            manHinhDen.alpha = 0;
        }
        foreach (GameObject p in nguoiChoiHienTai)
        {
            Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
            if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        dangChuyenCanh = false;
    }
}