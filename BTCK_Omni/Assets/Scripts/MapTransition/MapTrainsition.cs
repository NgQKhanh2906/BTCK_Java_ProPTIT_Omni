using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [Header("Thiết lập 2 Phòng đứng cạnh nhau")]
    public GameObject phongBenTrai;

    public GameObject phongBenPhai;

    private Collider2D myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            if (phongBenTrai != null) phongBenTrai.SetActive(true);
            if (phongBenPhai != null) phongBenPhai.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            KiemTraVaTatMap();
        }
    }

    private void KiemTraVaTatMap()
    {
        List<GameObject> nguoiChoiConSong = new List<GameObject>();
        nguoiChoiConSong.AddRange(GameObject.FindGameObjectsWithTag("Player1"));
        nguoiChoiConSong.AddRange(GameObject.FindGameObjectsWithTag("Player2"));

        foreach (GameObject player in nguoiChoiConSong)
        {
            if (myCollider.bounds.Contains(player.transform.position))
            {
                return;
            }
        }

        bool giuBenTrai = false;
        bool giuBenPhai = false;

        foreach (GameObject player in nguoiChoiConSong)
        {
            if (player.transform.position.x < transform.position.x)
            {
                giuBenTrai = true;
            }
            else if (player.transform.position.x > transform.position.x)
            {
                giuBenPhai = true;
            }
        }

        if (phongBenTrai != null) phongBenTrai.SetActive(giuBenTrai);
        if (phongBenPhai != null) phongBenPhai.SetActive(giuBenPhai);
    }
}