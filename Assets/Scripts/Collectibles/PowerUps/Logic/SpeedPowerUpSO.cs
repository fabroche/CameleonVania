using UnityEngine;

[CreateAssetMenu(fileName = "NewSpeedPowerUp", menuName = "PowerUps/SpeedBoost")]
public class SpeedPowerUpSO : PowerUpEffectSO
{
    [Header("Configuración Específica")]
    [Tooltip("Cantidad de velocidad a sumar o multiplicar")]
    
    public float speedMultiplier = 2.0f;
    
    public override void Apply(PlayerController target)
    {
        target.moveSpeed *= speedMultiplier;
        Debug.Log($"[PowerUp] {idName} Aplicado, Velocidad Actual: {target.moveSpeed}");
    }

    public override void Remove(PlayerController target)
    {
        target.moveSpeed /= speedMultiplier;
        Debug.Log($"[PowerUp] {idName} Terminado, Velocidad Retomada: {target.moveSpeed}");
    }
}