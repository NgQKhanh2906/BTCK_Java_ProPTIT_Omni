using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSensor : MonoBehaviour
{
    [Header("Trạng thái (Không cần tích)")]
    public bool coNguoi = false; 
    
    private int soNguoiTrongCua = 0; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            soNguoiTrongCua++;
            coNguoi = true; 
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            soNguoiTrongCua--;
            if (soNguoiTrongCua <= 0)
            {
                soNguoiTrongCua = 0;
                coNguoi = false;
            }
        }
    }
}