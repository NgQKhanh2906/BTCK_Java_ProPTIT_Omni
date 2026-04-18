using System;
using UnityEngine;
public interface ISaveable
{
    string UniqueId { get; }
    object CaptureState();
    void   RestoreState(object state);
}