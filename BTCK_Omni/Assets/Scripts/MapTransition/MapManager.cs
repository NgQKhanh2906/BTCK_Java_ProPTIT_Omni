using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Danh sách toàn bộ các Room (Kéo thả vào đây)")]
    public GameObject[] danhSachRooms; 

    [Header("Room hiển thị lúc mới vào game")]
    public int startRoom = 0; 
    
    private void Start()
    {
        for (int i = 0; i < danhSachRooms.Length; i++)
        {
            if (i == startRoom)
            {
                danhSachRooms[i].SetActive(true);
            }
            else
            {
                danhSachRooms[i].SetActive(false);
            }
        }
    }
}