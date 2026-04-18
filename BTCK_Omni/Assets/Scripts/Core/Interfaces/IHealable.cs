using System;
using UnityEngine;
public interface IHealable
{
    void RestoreHP(float amount);
    void RestoreMana(float amount);
}