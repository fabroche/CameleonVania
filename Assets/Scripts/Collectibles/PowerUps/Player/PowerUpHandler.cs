using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using _Project.Player;

// Handler para manejar los powerups, va en el GameObject del jugador
// Gestion los tiempos y los reinicios de los powerups
public class PowerUpHandler : MonoBehaviour
{
    public PlayerController _player;

    // Diccionario para almacenar los powerups activos y sus tiempos restantes
    private Dictionary<PowerUpEffectSO, CancellationTokenSource> _activePowerUps = new();

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    // Este metodo lo utiliza el GameObject que es un collectible
    public void ActivatePowerUp(PowerUpEffectSO powerUpData)
    {
        // 1. Logica de Reinicio (Refresh)

        if (_activePowerUps.TryGetValue(powerUpData, out CancellationTokenSource existingToken))
        {
            // Cancelan el Timer anterior
            // Al cancelarlo el "await" del timer Viejo
            // Salta al catch y muere sin quitar el efecto

            existingToken.Cancel();
            existingToken.Dispose();

            Debug.Log($"Refrescando tiempo de: {powerUpData.idName}");
        }
        else
        {
            // Si es nuevo aplicamos el efecto inical
            // ej: Velocidad x2
            powerUpData.Apply(_player);
        }

        // 2. Iniciamos un nuevo timer (Nuevo o refrescado)

        StartPowerUpTimer(powerUpData);
    }

    private async void StartPowerUpTimer(PowerUpEffectSO powerUpData)
    {
        var cts = new CancellationTokenSource();
        _activePowerUps[powerUpData] = cts;

        try
        {
            // Unimos el token del timer con el token de destruccion del objeto (por seguridad)

            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
                cts.Token,
                destroyCancellationToken
            );

            // Esperamos Async de Unity
            await Awaitable.WaitForSecondsAsync(
                powerUpData.duration,
                linkedToken.Token
            );

            // -- Si llegamos aqui es porque el timer termino normalmente

            powerUpData.Remove(_player);
            _activePowerUps.Remove(powerUpData);
        }
        catch (System.OperationCanceledException)
        {
            // Se cancelo el timer porque recogio otro powerup (Refresh)
            // No hacemos Nada (No removemos el efeceto), solo dejamos morir en esta rutina
        }
        finally
        {
            // Limpiamos el token
            cts.Dispose();
        }
    }
}