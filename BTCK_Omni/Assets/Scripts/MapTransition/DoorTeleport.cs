using UnityEngine;
using System.Collections;

public class InteractiveDoor : MonoBehaviour
{
    [Header("Thiết lập Phòng")]
    public GameObject phongCanTat;
    public GameObject phongCanBat;

    [Header("Thiết lập Vị trí & Màn hình")]
    public Transform viTriDichDen;
    public CanvasGroup manHinhDen;
    
    [Header("Giao diện (Kéo cái Nut_F vào đây)")]
    public GameObject hinhAnhBimF; 

    [Header("Thời gian")]
    public float thoiGianToi = 0.5f;
    
    private bool dungGanCua = false;
    private bool dangDichChuyen = false;

    
    private void Update()
    {
        if (dungGanCua && !dangDichChuyen && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(ChuyenCanhRoutine());
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dungGanCua = true;
            if (hinhAnhBimF != null) hinhAnhBimF.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dungGanCua = false;
            if (hinhAnhBimF != null) hinhAnhBimF.SetActive(false);
        }
    }

    private IEnumerator ChuyenCanhRoutine()
    {
        dangDichChuyen = true;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        float timer = 0;
        while (timer < thoiGianToi)
        {
            timer += Time.deltaTime;
            manHinhDen.alpha = timer / thoiGianToi;
            yield return null;
        }
        manHinhDen.alpha = 1;
        
        if (phongCanTat != null) phongCanTat.SetActive(false);
        if (phongCanBat != null) phongCanBat.SetActive(true);
        if (player != null) player.transform.position = viTriDichDen.position;
        
        if (hinhAnhBimF != null) hinhAnhBimF.SetActive(false);
        dungGanCua = false; 

        yield return new WaitForSeconds(0.2f);
        
        timer = 0;
        while (timer < thoiGianToi)
        {
            timer += Time.deltaTime;
            manHinhDen.alpha = 1 - (timer / thoiGianToi);
            yield return null;
        }
        manHinhDen.alpha = 0;

        dangDichChuyen = false;
    }
}