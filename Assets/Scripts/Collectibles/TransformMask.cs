using UnityEngine;

public class TransformMask : MonoBehaviour
{
    [Header("Transformation Settings")]
    [Tooltip("La transformación que otorga esta máscara al jugador")]
    [SerializeField]
    private TransformationData transformationType;

    [Header("Collection State")] private bool _hasBeenCollected = false;

    [Header("VFX/SFX - Optional")] 
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioClip collectSound;

    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (_hasBeenCollected) return;
        
        PlayerTransform playerTransform = other.GetComponent<PlayerTransform>();
        if (playerTransform == null) return;
        
        if (transformationType == null)
        {
            Debug.LogError("[TransformMask] No transformation type assigned!");
            return;
        }
        
        if (playerTransform._currentTransformation == transformationType)
        {
            Debug.Log($"[TransformMask] Player already has {transformationType.transformName} form. Ignoring.");
            return;
        }
        
        _hasBeenCollected = true;
        
        playerTransform.TransformInto(transformationType);
        
        Debug.Log($"[TransformMask] Player transformed into {transformationType.transformName}");
        
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
        
        Destroy(gameObject, 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        // Visualizar trigger range
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }
    }
    
    public void SetTransformationType(TransformationData newTransformation)
    {
        transformationType = newTransformation;
    }
    
    public TransformationData GetTransformationType() => transformationType;  
}