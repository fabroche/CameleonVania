using System;
using UnityEngine;

public class WaterEvents : MonoBehaviour
{
    public static event Action<GameObject, bool> OnPlayerEnterWater;
    
    public static event Action<GameObject> OnPlayerExitWater;
    
    public static event Action<GameObject, bool> OnSwimStateChanged;
    
    public static void PlayerEnterWater(GameObject player, bool canSwim)
    {
        OnPlayerEnterWater?.Invoke(player, canSwim);
    }
    
    public static void PlayerExitWater(GameObject player)
    {
        OnPlayerExitWater?.Invoke(player);
    }
    
    public static void SwimStateChanged(GameObject player, bool canSwim)
    {
        OnSwimStateChanged?.Invoke(player, canSwim);
    }
}