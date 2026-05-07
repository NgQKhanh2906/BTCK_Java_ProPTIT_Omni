using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    [Tooltip("Kéo CameraLimit của căn phòng đầu tiên (nơi nhân vật xuất hiện) vào đây")]
    public Collider2D startingCameraLimit;

    void Start()
    {
        if (CamController.Instance != null && startingCameraLimit != null)
        {
            CamController.Instance.ChangeLimit(startingCameraLimit);
            
            CamController.Instance.transform.position = new Vector3(
                CamController.Instance.transform.position.x,
                CamController.Instance.transform.position.y,
                -10f);
        }
    }
}