using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 5.0f;

    private Rigidbody2D _rb;
    private float _moveInput;
    private bool _isFacingRight = true;
    
    [Header("Debug")] [Tooltip("Mostrar logs de entrada/salida")] [SerializeField]
    private bool debugLogs = false;

    [Header("Jump Settings")] public float jumpForce = 10f;

    [Header("Ground Check")] public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool _isGrounded;

    [Header("Knockback/Stun")] private bool isStunned = false;
    private float stunEndTime = 0f;
    private float stunDuration = 0.3f;

    [Header("Swimming")] [Tooltip("Si está actualmente en agua")]
    private bool isInWater = false;

    [Tooltip("Fuerza de salto/natación en agua (más débil que salto normal)")] [SerializeField]
    private float swimJumpForce = 4f;

    [Tooltip("Cooldown entre saltos en agua")] [SerializeField]
    private float swimJumpCooldown = 0.3f;

    private float lastSwimJumpTime = -999f;
    
    [Header("Wall Climbing - Spider")]
    [Tooltip("Velocidad de escalado en paredes/techo")]
    [Range(0.1f, 3f)]
    [SerializeField] private float climbSpeed = 3f;

    [Tooltip("Distancia de raycast para detectar superficies escalables")]
    [SerializeField] private float wallCheckDistance = 0.6f;

    [Tooltip("Fuerza de wall jump (x: alejarse de pared, y: altura)")]
    [SerializeField] private Vector2 wallJumpForce = new Vector2(6f, 10f);

    [Header("Visual Feedback")]
    [Tooltip("Transform del model3DParent para rotar el modelo")]
    [SerializeField] private Transform visualTransform;

    [Tooltip("Velocidad de rotación del modelo (suavidad)")]
    [SerializeField] private float rotationSpeed = 10f;

    // Estado de climbing
    private bool _isClimbing = false;
    private Vector2 _currentSurfaceNormal = Vector2.zero;
    private float _originalGravity;
    private float _targetRotation = 0f; // Rotación objetivo del modelo

    // Flag Pattern: Comunicación Update → FixedUpdate
    private bool _shouldApplyClimbingPhysics = false;
    private Vector2 _climbingDirection = Vector2.zero;

    // Life Cycle Methods
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _originalGravity = _rb.gravityScale;

        // Obtener referencia automática desde PlayerTransform
        PlayerTransform pt = GetComponent<PlayerTransform>();
        if (pt != null && visualTransform == null)
        {
            visualTransform = pt.model3DParent;
            Debug.Log("[PlayerController] visualTransform auto-assigned to model3DParent");
        }

        // Verificar que se asignó
        if (visualTransform == null)
        {
            Debug.LogWarning("[PlayerController] visualTransform no asignado! Rotación visual no funcionará.");
        }
        
        // Ensure this GameObject has the "Player" tag
        ValidatePlayerTag();

        // Subscribe to knockback event
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnTakeDamageWithKnockback += HandleKnockback;
        }
    }
    
    void OnEnable()
    {
        // Suscribirse a eventos
        WaterEvents.OnPlayerEnterWater += HandleEnterWater;
        WaterEvents.OnPlayerExitWater += HandleExitWater;
        WaterEvents.OnSwimStateChanged += HandleSwimStateChange;
    }
    
    // Update is called once per frame
    void Update()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        _moveInput = Input.GetAxisRaw("Horizontal");
        Jump();
        
        // === WALL CLIMBING SYSTEM ===
        HandleClimbing();

        // Actualizar rotación visual (siempre, climbing o no)
        UpdateVisualRotation();
    }

    void FixedUpdate()
    {
        // Don't apply movement if stunned (allows knockback to work)
        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
            }

            return; // Skip movement during stun
        }

        // Aplicar física de climbing o movimiento normal
        if (_shouldApplyClimbingPhysics)
        {
            ApplyClimbingPhysics();
        }
        else
        {
            // Movimiento horizontal normal (solo si NO está climbing)
            _rb.linearVelocity = new Vector2(_moveInput * moveSpeed, _rb.linearVelocity.y);
        }
    }

    void OnDisable()
    {
        // Desuscribirse a eventos
        WaterEvents.OnPlayerEnterWater -= HandleEnterWater;
        WaterEvents.OnPlayerExitWater -= HandleExitWater;
        WaterEvents.OnSwimStateChanged -= HandleSwimStateChange;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnTakeDamageWithKnockback -= HandleKnockback;
        }
    }

    // Utils Methods
    
    private void HandleKnockback(Vector2 direction, float damage)
    {
        // Apply knockback and stun player
        if (_rb != null)
        {
            float knockbackForce = damage * 0.5f; // Proportional to damage
            _rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);

            isStunned = true;
            stunEndTime = Time.time + stunDuration;

            Debug.Log($"[Player] Knocked back! Stunned for {stunDuration}s");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Automatically assigns "Player" tag in the Editor
    /// </summary>
    private void OnValidate()
    {
        ValidatePlayerTag();
    }
#endif

    /// <summary>
    /// Validates and auto-assigns the "Player" tag
    /// </summary>
    private void ValidatePlayerTag()
    {
        if (!gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
            Debug.Log("[PlayerController] Auto-assigned 'Player' tag to GameObject.");
        }
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        }
        else if (Input.GetButtonDown("Jump") && isInWater)
        {
            if (Time.time >= lastSwimJumpTime + swimJumpCooldown)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, swimJumpForce);
                lastSwimJumpTime = Time.time;
            }
        }
    }

    public void SetInWater(bool inWater)
    {
        isInWater = inWater;
        Debug.Log($"[PlayerController] In water: {inWater}");
    }
    
    private void HandleEnterWater(GameObject player, bool canSwim)
    {
        if (player != gameObject) return;

        isInWater = canSwim;

        if (debugLogs)
        {
            Debug.Log($"[PlayerController] Entered water. CanSwim: {canSwim}");
        }
    }
    
    private void HandleExitWater(GameObject player)
    {
        if (player != gameObject) return;

        isInWater = false;

        if (debugLogs)
        {
            Debug.Log($"[PlayerController] Exited water");
        }
    }
    
    private void HandleSwimStateChange(GameObject player, bool canSwim)
    {
        if (player != gameObject) return;

        isInWater = canSwim;

        if (debugLogs)
        {
            Debug.Log($"[PlayerController] Swim state changed. CanSwim: {canSwim}");
        }
    }
    
    /// <summary>
    /// Detecta si hay una superficie escalable cerca (pared, techo).
    /// Usa 4 raycasts para detectar en todas las direcciones.
    /// La prioridad depende del input del player (hacia arriba = techo, horizontal = pared).
    /// </summary>
    /// <param name="surfaceNormal">Normal de la superficie detectada</param>
    /// <returns>True si hay superficie escalable</returns>
    private bool CheckClimbableSurface(out Vector2 surfaceNormal)
    {
        surfaceNormal = Vector2.zero;

        // Origin del raycast (centro del player)
        Vector2 origin = transform.position;

        // Raycast en 4 direcciones
        RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, wallCheckDistance, groundLayer);
        RaycastHit2D hitUp = Physics2D.Raycast(origin, Vector2.up, wallCheckDistance, groundLayer);
        // Raycast Down es opcional (para detectar si está en suelo y NO permitir climb)

        // 🔧 FIX: Smart priority basado en input del player
        float verticalInput = Input.GetAxis("Vertical");

        // Si está climbing Y presiona hacia arriba → Priorizar techo
        if (_isClimbing && verticalInput > 0.1f && hitUp.collider != null)
        {
            surfaceNormal = hitUp.normal;

            if (debugLogs)
            {
                Debug.Log($"[PlayerController] Surface detected UP (ceiling) - PRIORITY (input up). Normal: {surfaceNormal}");
            }

            return true;
        }

        // Prioridad normal: paredes primero, luego techo
        if (hitRight.collider != null)
        {
            surfaceNormal = hitRight.normal;

            if (debugLogs)
            {
                Debug.Log($"[PlayerController] Surface detected RIGHT. Normal: {surfaceNormal}");
            }

            return true;
        }
        else if (hitLeft.collider != null)
        {
            surfaceNormal = hitLeft.normal;

            if (debugLogs)
            {
                Debug.Log($"[PlayerController] Surface detected LEFT. Normal: {surfaceNormal}");
            }

            return true;
        }
        else if (hitUp.collider != null)
        {
            surfaceNormal = hitUp.normal;

            if (debugLogs)
            {
                Debug.Log($"[PlayerController] Surface detected UP (ceiling). Normal: {surfaceNormal}");
            }

            return true;
        }

        // No hay superficie escalable cerca
        return false;
    }
    
    /// <summary>
    /// Convierte el normal de una superficie a un ángulo de rotación.
    /// Usa Atan2 para calcular el ángulo del vector normal.
    /// </summary>
    /// <param name="normal">Vector normal de la superficie</param>
    /// <returns>Ángulo de rotación en grados (Euler Z)</returns>
    private float GetRotationFromNormal(Vector2 normal)
    {
        // Mathf.Atan2 devuelve el ángulo del vector en radianes
        // Convertimos a grados con Mathf.Rad2Deg
        float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;

        // Ajustar para la orientación del sprite
        // Esto depende de cómo esté orientado tu modelo por defecto
        // Si el sprite mira hacia arriba (0° = up), sumamos 90°
        float rotation = angle + 90f;

        // Casos especiales para superficies ortogonales
        float tolerance = 0.1f;

        // PARED DERECHA: normal ≈ (-1, 0)
        if (Mathf.Abs(normal.x + 1f) < tolerance && Mathf.Abs(normal.y) < tolerance)
        {
            return -90f; // Rotar 90° a la derecha
        }

        // PARED IZQUIERDA: normal ≈ (1, 0)
        else if (Mathf.Abs(normal.x - 1f) < tolerance && Mathf.Abs(normal.y) < tolerance)
        {
            return 90f; // Rotar 90° a la izquierda
        }

        // TECHO: normal ≈ (0, -1)
        else if (Mathf.Abs(normal.y + 1f) < tolerance && Mathf.Abs(normal.x) < tolerance)
        {
            return 180f; // Boca abajo
        }

        // SUELO: normal ≈ (0, 1)
        else if (Mathf.Abs(normal.y - 1f) < tolerance && Mathf.Abs(normal.x) < tolerance)
        {
            return 0f; // Posición normal
        }

        // DEFAULT: Superficie diagonal (rampa, etc.)
        return rotation;
    }
    
    /// <summary>
    /// Calcula la dirección de movimiento en climbing (NO aplica física).
    /// Llamado desde Update() para calcular, física se aplica en FixedUpdate().
    /// </summary>
    /// <param name="surfaceNormal">Normal de la superficie actual</param>
    /// <param name="verticalInput">Input vertical del player</param>
    /// <param name="horizontalInput">Input horizontal del player</param>
    /// <returns>Vector2 con la dirección de movimiento</returns>
    private Vector2 CalculateClimbingDirection(Vector2 surfaceNormal, float verticalInput, float horizontalInput)
    {
        // Determinar tipo de superficie basado en el normal
        // PARED: normal.x significativo (apunta horizontal)
        bool isWall = Mathf.Abs(surfaceNormal.x) > 0.5f;

        // TECHO: normal.y apunta hacia abajo (< -0.5)
        bool isCeiling = surfaceNormal.y < -0.5f;

        Vector2 climbDirection = Vector2.zero;

        if (isCeiling)
        {
            // TECHO: Movimiento horizontal (left/right)
            climbDirection = new Vector2(horizontalInput * climbSpeed, 0f);

            if (debugLogs && verticalInput != 0f)
            {
                Debug.Log("[PlayerController] En techo - Input Vertical ignorado");
            }
        }
        else if (isWall)
        {
            // PARED: Movimiento vertical (up/down)
            // Mantener velocidad horizontal actual (no interferir con movimiento lateral)
            climbDirection = new Vector2(_rb.linearVelocity.x, verticalInput * climbSpeed);

            // Opcional: Cambiar facing direction con horizontal
            if (horizontalInput > 0.1f)
            {
                _isFacingRight = true;
            }
            else if (horizontalInput < -0.1f)
            {
                _isFacingRight = false;
            }
        }
        else
        {
            // SUELO u otra superficie: No debería estar climbing
            Debug.LogWarning("[PlayerController] Climbing en superficie no reconocida");
            climbDirection = Vector2.zero;
        }

        return climbDirection;
    }

    /// <summary>
    /// Aplica física de climbing (llamado desde FixedUpdate).
    /// </summary>
    private void ApplyClimbingPhysics()
    {
        if (_shouldApplyClimbingPhysics)
        {
            // APLICAR velocidad calculada en Update()
            _rb.linearVelocity = _climbingDirection;

            if (debugLogs)
            {
                Debug.Log($"[FixedUpdate] Applying climbing velocity: {_climbingDirection}");
            }
        }
    }
    
    /// <summary>
    /// Maneja el wall jump (saltar desde la pared).
    /// Player se despega de la pared con impulso horizontal + vertical.
    /// </summary>
    private void WallJump()
    {
        // Verificar que está climbing y presiona Jump
        if (Input.GetButtonDown("Jump") && _isClimbing)
        {
            // Salir del estado climbing
            _isClimbing = false;

            // Restaurar gravedad normal
            _rb.gravityScale = _originalGravity;

            // 🔧 FIX: Desactivar flag de climbing physics
            _shouldApplyClimbingPhysics = false;

            // Calcular dirección horizontal del salto
            // Si está mirando derecha (pared a la derecha), saltar hacia la izquierda
            // Si está mirando izquierda (pared a la izquierda), saltar hacia la derecha
            float jumpDirX = _isFacingRight ? -1f : 1f;

            // Aplicar velocidad de wall jump
            Vector2 wallJumpVelocity = new Vector2(
                jumpDirX * wallJumpForce.x,  // Horizontal: alejarse de pared
                wallJumpForce.y               // Vertical: altura del salto
            );

            _rb.linearVelocity = wallJumpVelocity;

            if (debugLogs)
            {
                Debug.Log($"[PlayerController] Wall Jump! Direction: {jumpDirX}, Velocity: {wallJumpVelocity}");
            }
        }
    }
    
    /// <summary>
    /// Maneja todo el sistema de wall climbing.
    /// Verifica habilidad, detecta superficies, activa/desactiva climbing y wall jump.
    /// </summary>
    private void HandleClimbing()
    {
        // Obtener componente PlayerTransform
        PlayerTransform pt = GetComponent<PlayerTransform>();

        // Verificar si tiene habilidad de wall climbing (Spider transformation)
        if (pt != null && pt.CanWallClimb())
        {
            // 🔧 FIX: Forzar salida si toca el suelo (evita quedarse pegado)
            if (_isGrounded && _isClimbing)
            {
                DeactivateClimbing();
                return; // Salir temprano, no seguir verificando
            }

            // Detectar superficie escalable cerca
            bool hasSurface = CheckClimbableSurface(out Vector2 surfaceNormal);

            // Input de movimiento (¿quiere escalar?)
            float verticalInput = Input.GetAxis("Vertical");
            float horizontalInput = Input.GetAxis("Horizontal");

            // 🔧 FIX: Solo continuar climbing si HAY input de movimiento
            // Esto permite "soltar" el techo al dejar de presionar teclas
            bool hasMovementInput = Mathf.Abs(verticalInput) > 0.1f || Mathf.Abs(horizontalInput) > 0.1f;

            // Si ya está climbing, requiere input para continuar (permite "soltar")
            // Si no está climbing, solo activar si hay input
            bool wantsToClimb = hasMovementInput;

            // NO permitir climbing si está en el suelo (evitar "pegarse" al caminar)
            bool canStartClimbing = !_isGrounded;

            // Decidir si activar climbing
            if (hasSurface && wantsToClimb && canStartClimbing)
            {
                ActivateClimbing(surfaceNormal);
            }
            else
            {
                DeactivateClimbing();
            }

            // Wall Jump (siempre verificar, incluso si ya no está climbing)
            WallJump();
        }
        else
        {
            // No tiene transformación Spider → Forzar salir de climbing
            ForceExitClimbing();
        }
    }
    
    /// <summary>
    /// Activa el estado de climbing y calcula dirección de movimiento.
    /// La física se aplica en FixedUpdate().
    /// </summary>
    private void ActivateClimbing(Vector2 surfaceNormal)
    {
        // ACTIVAR CLIMBING (si no estaba ya activo)
        if (!_isClimbing)
        {
            _isClimbing = true;
            _rb.gravityScale = 0f; // Anular gravedad

            if (debugLogs)
            {
                Debug.Log("[PlayerController] Started climbing");
            }
        }

        // Guardar normal actual
        _currentSurfaceNormal = surfaceNormal;

        // Leer input
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        // CALCULAR dirección (no aplicar todavía)
        _climbingDirection = CalculateClimbingDirection(surfaceNormal, verticalInput, horizontalInput);

        // Activar flag para aplicar en FixedUpdate
        _shouldApplyClimbingPhysics = true;
    }

    /// <summary>
    /// Desactiva el estado de climbing y restaura gravedad.
    /// </summary>
    private void DeactivateClimbing()
    {
        if (_isClimbing)
        {
            _isClimbing = false;
            _rb.gravityScale = _originalGravity; // Restaurar gravedad
            _shouldApplyClimbingPhysics = false; // Desactivar flag

            if (debugLogs)
            {
                Debug.Log("[PlayerController] Stopped climbing");
            }
        }
    }

    /// <summary>
    /// Fuerza salida de climbing cuando se pierde la transformación Spider.
    /// </summary>
    private void ForceExitClimbing()
    {
        if (_isClimbing)
        {
            _isClimbing = false;
            _rb.gravityScale = _originalGravity;
            _shouldApplyClimbingPhysics = false; // Desactivar flag

            if (debugLogs)
            {
                Debug.Log("[PlayerController] Lost Spider transformation - climbing disabled");
            }
        }
    }

    /// <summary>
    /// Actualiza la rotación del modelo visual según el estado.
    /// - Climbing: Rota según superficie (pared/techo)
    /// - Normal: Rotación 0° (de pie)
    /// Usa Lerp para transición suave.
    /// </summary>
    private void UpdateVisualRotation()
    {
        // Verificar que visualTransform está asignado
        if (visualTransform == null)
        {
            return;
        }

        // Determinar rotación objetivo según estado
        if (_isClimbing)
        {
            // Calcular rotación basada en el normal de la superficie
            _targetRotation = GetRotationFromNormal(_currentSurfaceNormal);
        }
        else
        {
            // Estado normal: sin rotación (de pie)
            _targetRotation = 0f;
        }

        // Obtener rotación actual (solo eje Z, 2D)
        float currentZ = visualTransform.eulerAngles.z;

        // Normalizar ángulo a rango -180 a 180
        // (Unity devuelve 0-360, pero para Lerp es mejor -180 a 180)
        if (currentZ > 180f)
        {
            currentZ -= 360f;
        }

        // Interpolar suavemente entre rotación actual y objetivo
        // Mathf.LerpAngle maneja correctamente el wrap-around (359° → 0°)
        float newZ = Mathf.LerpAngle(currentZ, _targetRotation, rotationSpeed * Time.deltaTime);

        // Aplicar rotación solo en eje Z (2D)
        visualTransform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Visualizar raycasts de climbing (solo si tiene transformación Spider)
        PlayerTransform pt = GetComponent<PlayerTransform>();
        if (pt == null || !pt.CanWallClimb()) return;

        Vector2 origin = transform.position;

        // Color de los raycasts
        Gizmos.color = Color.cyan;

        // Raycast Right
        Gizmos.DrawLine(origin, origin + Vector2.right * wallCheckDistance);

        // Raycast Left
        Gizmos.DrawLine(origin, origin + Vector2.left * wallCheckDistance);

        // Raycast Up
        Gizmos.DrawLine(origin, origin + Vector2.up * wallCheckDistance);

        // Si está climbing, dibujar normal de la superficie
        if (_isClimbing)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + _currentSurfaceNormal * 2f);
        }
    }
#endif
}