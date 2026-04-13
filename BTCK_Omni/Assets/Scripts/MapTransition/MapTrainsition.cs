using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [Header("Thiết lập 2 Phòng đứng cạnh nhau")]
    public GameObject phongBenTrai;  
    public GameObject phongBenPhai;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (phongBenTrai != null) phongBenTrai.SetActive(true);
            if (phongBenPhai != null) phongBenPhai.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.transform.position.x > transform.position.x)
            {
                if (phongBenTrai != null) phongBenTrai.SetActive(false);
            }
            else if (other.transform.position.x < transform.position.x)
            {
                if (phongBenPhai != null) phongBenPhai.SetActive(false);
            }
        }
    }
}