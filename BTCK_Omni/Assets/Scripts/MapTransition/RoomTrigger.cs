using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public Collider2D roomLimit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (CamController.Instance != null)
            {
                CamController.Instance.ChangeLimit(roomLimit);
            }
        }
    }
}