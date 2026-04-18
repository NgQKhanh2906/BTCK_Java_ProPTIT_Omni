using System;
using UnityEngine;
public interface IInteractable
{
    bool CanInteract { get; }
    void Interact(PlayerBase player);
}