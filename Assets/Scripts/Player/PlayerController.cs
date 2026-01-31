using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 5.0f;

    private Rigidbody2D _rb;
    private float _moveInput;
    
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

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

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

    void OnDisable()
    {
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

    // Update is called once per frame
    void Update()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        _moveInput = Input.GetAxisRaw("Horizontal");
        Jump();
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

        _rb.linearVelocity = new Vector2(_moveInput * moveSpeed, _rb.linearVelocity.y);
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
    
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}