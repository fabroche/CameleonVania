using System;
using UnityEngine;
// Este Script va en el prefab del collectible que activa el powerup
public class PowerUpPickup : MonoBehaviour
{
    [Header("Dats del Powerup")] [Tooltip("Arrastra aqui el archivo .asset que creaste para el SpeedBoost")]

    public PowerUpEffectSO effectToApply;
    
    [Header("Feedback")]
    public bool destroyOnPickup = true;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Buscamos si lo que nos toco tiene el sistema de PowerUps
        
        if (other.TryGetComponent(out PowerUpHandler handler))
        {
            // 2. Le entregamos la data
            handler.ActivatePowerUp(effectToApply);
            
            // 3. Feedback
            
            // 4. Destruimos el objeto
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}