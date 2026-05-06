using UnityEngine;

public class ReviveStation : MonoBehaviour, IInteractable
{
    public Transform pt;
    private bool used = false;

    public bool CanInteract => !used;

    public void Interact(PlayerBase p)
    {
        if (used) return;

        PlayerBase p1 = null;
        PlayerBase p2 = null;

        PlayerBase[] arr = FindObjectsOfType<PlayerBase>();
        foreach (PlayerBase x in arr)
        {
            if (x.playerIndex == 1) p1 = x;
            if (x.playerIndex == 2) p2 = x;
        }

        PlayerBase t = (p.playerIndex == 1) ? p2 : p1;

        if (t && t.IsDead() && t.isCorpseLost)
        {
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.SetLivesDirectly(t.playerIndex, 1);
            }
            LivesManager.Instance.TriggerRespawn(t, pt.position);
            used = true;
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}