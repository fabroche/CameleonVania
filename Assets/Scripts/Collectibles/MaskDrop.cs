using UnityEngine;

public class MaskDrop : MonoBehaviour
{
    // Variables aquí
    [Header("Mask Drop Settings")]
    [Tooltip("Prefab de la máscara a instanciar (TransformMask_Frog, etc.)")]
    [SerializeField] private GameObject maskPrefab;

    [Tooltip("Transformación que otorgará la máscara")]
    [SerializeField] private TransformationData transformToDropData;
    
    [Header("Spawn Settings")]
    [Tooltip("Offset vertical del spawn (para que aparezca encima del enemy)")]
    [SerializeField] private float spawnHeightOffset = 0.5f;
    
    [Header("Component References")]
    private Health _health;
    
    
    
    void Start()
    {
        _health = GetComponent<Health>();
        if (_health == null)
        {
            Debug.LogError("[MaskDrop] Health component not found!");
            enabled = false;
            return;
        }
        _health.OnDeath += DropMask;
        Debug.Log($"[MaskDrop] {gameObject.name} ready to drop {transformToDropData?.transformName ?? "Unknown"} mask");
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDeath -= DropMask;
            Debug.Log($"[MaskDrop] Unsubscribed from {gameObject.name} OnDeath event");
        }
    }

    void DropMask()
    {
        if (maskPrefab == null)
        {
            Debug.LogWarning($"[MaskDrop] {gameObject.name} has no mask prefab assigned!");
            return;
        }
        
        if (transformToDropData == null)
        {
            Debug.LogWarning($"[MaskDrop] {gameObject.name} has no transformation data assigned!");
            return;
        }
        
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeightOffset;
        
        GameObject maskInstance = Instantiate(
            maskPrefab,
            spawnPosition,
            Quaternion.identity // Sin rotación
        );

        // Configurar el TransformationData
        TransformMask maskComponent = maskInstance.GetComponent<TransformMask>();
        
        if (maskComponent != null)
        {
            maskComponent.SetTransformationType(transformToDropData);
            Debug.Log($"[MaskDrop] {gameObject.name} dropped {transformToDropData.transformName} mask");
        }
        else
        {
            Debug.LogError($"[MaskDrop] Spawned mask doesn't have TransformMask component!");
        }
    }
}