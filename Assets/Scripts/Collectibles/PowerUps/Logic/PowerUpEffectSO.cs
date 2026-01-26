using _Project.Player;
using UnityEngine;

public abstract class PowerUpEffectSO : ScriptableObject
{
    [Header("Configuración General")] 
    public string idName;
    public float duration;

    public abstract void Apply(PlayerController target);
    public abstract void Remove(PlayerController target);
}