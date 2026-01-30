using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Transformation", menuName = "CameleonVania/Transformation", order = 0)]
public class TransformationData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Nombre de la transformación (ej: Frog, Spider, Ladybug)")]
    public string transformName = "New Transformation";
    
    [Tooltip("Prefab del modelo 3D que se instanciará al transformarse")]
    public GameObject modelPrefab;
    
    [Header("Stats Modifiers")]
    [Tooltip("Multiplicador de velocidad (1.0 = normal, 0.5 = mitad, 2.0 = doble)")]
    [Range(0.1f, 3.0f)]
    public float speedMultiplier = 1.0f;
    
    [Tooltip("Multiplicador de salto (1.0 = normal, 0.5 = mitad, 2.0 = doble)")]
    [Range(0.1f, 3.0f)]
    public float jumpMultiplier = 1.0f;
    
    [Tooltip("Multiplicador de tamañoY (1.0 = normal, 0.5 = mitad, 2.0 = doble)")]
    [Range(0.1f, 3.0f)]
    public float heightMultiplier = 1.0f;
    
    [Tooltip("Multiplicador de tamañoX (1.0 = normal, 0.5 = mitad, 2.0 = doble)")]
    [Range(0.1f, 3.0f)]
    public float widthMultiplier = 1.0f;
    
    [Tooltip("Multiplicador de Vida (1.0 = normal, 0.5 = mitad, 2.0 = doble)")]
    [Range(0.1f, 3.0f)]
    public float healthMultiplier = 1.0f;
    
    [Tooltip("Multiplicador de Daño (1.0 = normal, 0.5 = mitad, 2.0 = doble)")]
    [Range(0.1f, 3.0f)]
    public float damageMultiplier = 1.0f;
    
    [Tooltip("Multiplicador de velocidad de ataque (1.0 = normal, 0.5 = mitad, 2.0 = doble)")]
    [Range(0.1f, 3.0f)]
    public float attackSpeedMultiplier = 1.0f;
    
    [Tooltip("Multiplicador de Rango de ataque (1.0 = normal, 0.5 = mitad, 2.0 = doble)")]
    [Range(0.1f, 3.0f)]
    public float attackRangeMultiplier = 1.0f;
    
    [Header("Special Abilities")]
    [Tooltip("Puede nadar en zonas de agua (Frog)")]
    public bool canSwim = false;
    
    [Tooltip("Puede trepar paredes (Spider)")]
    public bool canWallClimb = false;
    
    [Tooltip("Puede pasar por espacios pequeños (Ladybug)")]
    public bool canFitSmallGaps = false;
    
    [Header("UI")]
    [Tooltip("Ícono de la transformación para mostrar en el HUD")]
    public Sprite transformIcon;
    
    [Tooltip("Descripción de la transformación")]
    [TextArea(3, 5)]
    public string description = "Descripción de la transformación";

    private void OnValidate()
    {
        // Asegurar que todos los multipliers estén dentro del rango válido
        speedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 3.0f);
        jumpMultiplier = Mathf.Clamp(jumpMultiplier, 0.1f, 3.0f);
        heightMultiplier = Mathf.Clamp(heightMultiplier, 0.1f, 3.0f);
        widthMultiplier = Mathf.Clamp(widthMultiplier, 0.1f, 3.0f);
        healthMultiplier = Mathf.Clamp(healthMultiplier, 0.1f, 3.0f);
        damageMultiplier = Mathf.Clamp(damageMultiplier, 0.1f, 3.0f);
        attackSpeedMultiplier = Mathf.Clamp(attackSpeedMultiplier, 0.1f, 3.0f);
    }
}