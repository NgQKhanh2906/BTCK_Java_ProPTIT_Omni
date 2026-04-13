using System.Collections;
using UnityEngine;
using System.Collections;

public class DoorTeleport : MonoBehaviour
{
    [Header("Thiết lập Phòng")]
    public GameObject phongCanTat; 
    public GameObject phongCanBat; 
    
    [Header("Thiết lập Vị trí & Màn hình")]
    public Transform viTriDichDen;
    public CanvasGroup manHinhDen; 
    
    [Header("Thời gian")]
    public float thoiGianToi = 1f; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ChuyenCanhRoutine(other.gameObject));
        }
    }

    private IEnumerator ChuyenCanhRoutine(GameObject player)
    {
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
        player.transform.position = viTriDichDen.position;
        yield return new WaitForSeconds(0.2f); 
        
        timer = 0;
        while (timer < thoiGianToi)
        {
            timer += Time.deltaTime;
            manHinhDen.alpha = 1 - (timer / thoiGianToi);
            yield return null;
        }
        manHinhDen.alpha = 0;
    }
}