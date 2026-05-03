using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Danh sách toàn bộ các Room (Kéo thả vào đây)")]
    public GameObject[] danhSachRooms;

    [Header("Room hiển thị lúc mới vào game")]
    public int startRoom = 0;

    private int curIdx;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.IsLoading())
        {
            SetRoom(SaveSystem.Instance.GetPendingRoomIdx());
        }
        else
        {
            SetRoom(startRoom);
        }
    }

    public void SetRoom(int idx)
    {
        if (danhSachRooms == null || idx < 0 || idx >= danhSachRooms.Length) return;

        for (int i = 0; i < danhSachRooms.Length; i++)
        {
            if (i == idx)
                danhSachRooms[i].SetActive(true);
            else danhSachRooms[i].SetActive(false);
        }
    }

    public int GetCurIdx()
    {
        if (danhSachRooms == null) return 0;

        for (int i = 0; i < danhSachRooms.Length; i++)
        {
            if (danhSachRooms[i] != null && danhSachRooms[i].activeInHierarchy)
            {
                return i;
            }
        }

        return 0;
    }
}