using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public Collider2D roomLimit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Player1") || other.CompareTag("Player2")))
        {
            if (CamController.Instance != null)
            {
                CamController.Instance.ChangeLimit(roomLimit);
            }
        }
    }
}