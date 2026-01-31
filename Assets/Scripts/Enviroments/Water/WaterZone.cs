using System.Collections.Generic;
using UnityEngine;

public class WaterZone : MonoBehaviour
{
    [Header("Water Settings")]
    [Tooltip("Daño por segundo para player sin transformación Frog (999 = muerte instantánea)")]
    [SerializeField]
    private float damagePerSecond = 999f;

    [Tooltip("Gravedad mientras nada (0.5 = mitad de gravedad normal)")] [SerializeField]
    private float swimGravityScale = 0.5f;

    [Header("Debug")] [Tooltip("Mostrar logs de entrada/salida")] [SerializeField]
    private bool debugLogs = false;

    [Header("Runtime Data")]
    // Dictionary para guardar gravedad original de cada objeto que entra
    private Dictionary<int, float> originalGravities = new Dictionary<int, float>();
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar que es el player
        PlayerTransform pt = other.GetComponent<PlayerTransform>();
        if (pt == null) return;

        // Obtener Rigidbody2D
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("[WaterZone] Player doesn't have Rigidbody2D!");
            return;
        }

        // Guardar gravedad original
        int instanceID = other.gameObject.GetInstanceID();
        if (!originalGravities.ContainsKey(instanceID))
        {
            originalGravities[instanceID] = rb.gravityScale;

            if (debugLogs)
            {
                Debug.Log($"[WaterZone] {other.name} entered water. Original gravity: {rb.gravityScale}");
            }
        }

        WaterEvents.PlayerEnterWater(other.gameObject, pt.CanSwim());
        if (debugLogs)
        {
            Debug.Log($"[WaterZone] {other.name} entered water. CanSwim: {pt.CanSwim()}");
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerTransform pt = other.GetComponent<PlayerTransform>();
        if (pt == null) return;

        if (pt.CanSwim())
        {
            ApplySwimPhysics(other);

            WaterEvents.SwimStateChanged(other.gameObject, true);
        }
        else
        {
            ApplyWaterDamage(other);

            WaterEvents.SwimStateChanged(other.gameObject, false);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Verificar que es el player
        PlayerTransform pt = other.GetComponent<PlayerTransform>();
        if (pt == null) return;

        // Obtener instanceID
        int instanceID = other.gameObject.GetInstanceID();

        // Restaurar gravedad original si existe
        if (originalGravities.ContainsKey(instanceID))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float restoredGravity = originalGravities[instanceID];
                rb.gravityScale = restoredGravity;

                if (debugLogs)
                {
                    Debug.Log($"[WaterZone] {other.name} exited water. Restored gravity: {restoredGravity}");
                }
            }

            // Limpiar del Dictionary
            originalGravities.Remove(instanceID);
        }

        // Disparar evento de salida del agua
        WaterEvents.PlayerExitWater(other.gameObject);

        if (debugLogs)
        {
            Debug.Log($"[WaterZone] {other.name} exited water");
        }
    }

    private void ApplySwimPhysics(Collider2D playerCollider)
    {
        Rigidbody2D rb = playerCollider.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Reducir gravedad para simular flotación
        rb.gravityScale = swimGravityScale;

        if (debugLogs)
        {
            Debug.Log($"[WaterZone] {playerCollider.name} swimming (gravity: {swimGravityScale})");
        }
    }

    private void ApplyWaterDamage(Collider2D playerCollider)
    {
        Health health = playerCollider.GetComponent<Health>();
        if (health == null) return;

        // Calcular daño de este frame
        float damageThisFrame = damagePerSecond * Time.deltaTime;

        // Aplicar daño (sin knockback en agua)
        health.TakeDamage(damageThisFrame, Vector2.zero);

        if (debugLogs)
        {
            Debug.Log($"[WaterZone] {playerCollider.name} taking water damage: {damageThisFrame:F2}");
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualizar zona de agua en Scene view
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            // Color azul semi-transparente
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.offset, col.size);

            // Borde azul más oscuro
            Gizmos.color = new Color(0f, 0.3f, 0.8f, 0.8f);
            Gizmos.DrawWireCube(col.offset, col.size);
        }
    }
}