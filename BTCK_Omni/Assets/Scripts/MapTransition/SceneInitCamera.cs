using System.Collections;
using UnityEngine;

public class SceneInitCamera : MonoBehaviour
{
    [Header("Khu vực bắt đầu của Map")]
    [Tooltip("Kéo BoxCollider2D của Room mà nhân vật vừa mới spawn vào đây")]
    public Collider2D startingRoomLimit;

    void Start()
    {
        StartCoroutine(InitCameraDelay());
    }

    IEnumerator InitCameraDelay()
    {
        yield return null; 

        if (CamController.Instance != null && startingRoomLimit != null)
        {
            CamController.Instance.ChangeLimit(startingRoomLimit);
            Vector3 startPos = new Vector3(startingRoomLimit.bounds.center.x, startingRoomLimit.bounds.center.y, -10f);
            CamController.Instance.SnapCamera(startPos);
            if (CamController.Instance.vcam != null)
            {
                CamController.Instance.vcam.PreviousStateIsValid = false;
            }
        }
    }
}