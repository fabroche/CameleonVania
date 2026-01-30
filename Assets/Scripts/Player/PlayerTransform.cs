using UnityEngine;

public class PlayerTransform : MonoBehaviour
{
    [Header("Current Transformation")] [SerializeField]
    public TransformationData baseForm;

    public TransformationData _currentTransformation;

    [SerializeField] private GameObject currentModel;

    [Header("Component References")] private PlayerController _playerController;
    private Rigidbody2D _rb;
    private Health _health;
    private PlayerAttack _playerAttack;

    [Header("Base Stats")] private float _baseMoveSpeed;
    private float _baseJumpForce;
    private float _baseMaxHealth;
    private float _baseAttackDamage;
    private float _baseAttackCooldown;
    private float _baseAttackRange;
    private Vector3 _baseScale;

    [Header("Model Parent")] 
    [Tooltip("Transform donde se instanciará el modelo (hijo del player)")]
    public Transform model3DParent;
    
    [Header("Base Model")]
    [Tooltip("SpriteRenderer del modelo base del player (se oculta al transformarse)")]
    [SerializeField] private SpriteRenderer baseModelRenderer;

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _playerAttack = GetComponent<PlayerAttack>();
        _currentTransformation = baseForm;

        if (_playerController == null)
        {
            Debug.LogError("PlayerTransform: PlayerController no encontrado!");
            return;
        }

        if (_rb == null)
        {
            Debug.LogError("PlayerTransform: Rigidbody2D no encontrado!");
            return;
        }

        if (_health == null)
        {
            Debug.LogError("PlayerTransform: Health no encontrado!");
            return;
        }

        if (_playerAttack == null)
        {
            Debug.LogError("PlayerTransform: PlayerAttack no encontrado!");
            return;
        }

        _baseMoveSpeed = _playerController.moveSpeed;
        _baseJumpForce = _playerController.jumpForce;
        _baseMaxHealth = _health.GetMaxHealth();
        _baseAttackDamage = _playerAttack.GetAttackDamage();
        _baseAttackCooldown = _playerAttack.GetAttackCooldown();
        _baseAttackRange = _playerAttack.GetAttackRange();
        _baseScale = transform.localScale;

        Debug.Log(
            $"PlayerTransform initialized. Base speed: {_baseMoveSpeed}, Base jump: {_baseJumpForce}, Base max health: {_baseMaxHealth}, Base attack damage: {_baseAttackDamage}, Base attack cooldown: {_baseAttackCooldown}, Base attack range: {_baseAttackRange}, Base scale: {_baseScale}");
        
        TransformInto(baseForm);
    }

    public void TransformInto(TransformationData newTransformation)
    {
        // Destruir modelo anterior si existe
        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
        }

        // Instanciar nuevo modelo si existe
        if (newTransformation != null && newTransformation.modelPrefab != null)
        {
            currentModel = Instantiate(
                newTransformation.modelPrefab,
                model3DParent.position,
                model3DParent.rotation,
                model3DParent
            );
            
            // Ocultar modelo base cuando hay transformación
            if (baseModelRenderer != null)
            {
                baseModelRenderer.enabled = false;
                Debug.Log("[PlayerTransform] Base model hidden (transformed)");
            }
        }
        else
        {
            // Mostrar modelo base cuando NO hay transformación (base form)
            if (baseModelRenderer != null)
            {
                baseModelRenderer.enabled = true;
                Debug.Log("[PlayerTransform] Base model shown (base form)");
            }
        }

        ApplyTransformationStats(newTransformation);

        _currentTransformation = newTransformation;

        Debug.Log($"Transformed into: {newTransformation.transformName}");
    }

    private void ApplyTransformationStats(TransformationData transformationData)
    {
        // Movement
        _playerController.moveSpeed = _baseMoveSpeed * transformationData.speedMultiplier;
        _playerController.jumpForce = _baseJumpForce * transformationData.jumpMultiplier;

        // Scale
        transform.localScale = new Vector3(
            _baseScale.x * transformationData.widthMultiplier,
            _baseScale.y * transformationData.heightMultiplier,
            _baseScale.z
        );

        // Health (necesitas agregar SetMaxHealth en Health.cs)
        _health.SetMaxHealth(_baseMaxHealth * transformationData.healthMultiplier);

        // Attack (necesitas hacer públicos o agregar setters)
        _playerAttack.SetAttackDamage(_baseAttackDamage * transformationData.damageMultiplier);
        _playerAttack.SetAttackCooldown(_baseAttackCooldown / transformationData.attackSpeedMultiplier);
        _playerAttack.SetAttackRange(_baseAttackRange * transformationData.attackRangeMultiplier);
    }

    public void RevertToBaseForm()
    {
        TransformInto(baseForm);
    }

    public bool CanSwim() => _currentTransformation?.canSwim ?? false;
    public bool CanWallClimb() => _currentTransformation?.canWallClimb ?? false;
    public bool CanFitSmallGaps() => _currentTransformation?.canFitSmallGaps ?? false;
}