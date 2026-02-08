using UnityEngine;

public class LadyBugAI : MonoBehaviour
{
    [SerializeField]
    private enum State
    {
        Idle,
        Patrol,
        Flee, // Huir del player
        Stunned
    }

    private State _currentState;

    [Header("Movement")] [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool _movingRight = true;

    [Header("Detection")] [SerializeField] private float detectionRange = 5f;
    private float _fleeRangeHysteresis = 1.5f; // Player debe alejarse más para dejar de huir

    [Header("Jump Over Player")] [Tooltip("Fuerza del salto evasivo")] [SerializeField]
    private float jumpForce = 8f;

    [Tooltip("Distancia mínima al player para intentar saltar sobre él")] [SerializeField]
    private float jumpOverDistance = 1.5f;

    [Tooltip("Cooldown entre saltos")] [SerializeField]
    private float jumpCooldown = 1f;

    private float _lastJumpTime = -999f;

    [Header("Knockback/Stun")] [SerializeField]
    private float stunDuration = 0.5f;

    private float _stunEndTime = 0f;
    private State _previousState = State.Patrol;

    [Header("Ground Check (Self)")] [Tooltip("Transform para verificar si LadyBug está en el suelo")] [SerializeField]
    private Transform groundCheckPoint;

    [Tooltip("Radio del ground check de LadyBug")] [SerializeField]
    private float groundCheckRadiusSelf = 0.2f;

    [Tooltip("Layer del suelo (Ground)")] [SerializeField]
    private LayerMask groundLayer;

    [Header("References")] private Rigidbody2D _rb;
    private Transform _player;
    private Vector2 _startPosition;
    private bool _isGrounded;

    [Header("Debug")] [Tooltip("Mostrar logs de entrada/salida")] [SerializeField]
    private bool debugLogs = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[LadyBugAI] Player not found! Make sure Player has 'Player' tag.");
        }

        _startPosition = transform.position;

        // Subscribe to knockback event
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnTakeDamageWithKnockback += HandleKnockback;
        }

        ChangeState(State.Patrol);
    }

    void OnDestroy()
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnTakeDamageWithKnockback -= HandleKnockback;
        }
    }

    void Update()
    {
        // Verificar si está en el suelo
        _isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadiusSelf, groundLayer);

        switch (_currentState)
        {
            case State.Idle:
                Vector2 directiontoPlayer = (_player.position - transform.position).normalized;
                float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
                TryJumpOverPlayer(directiontoPlayer, distanceToPlayer);
                break;

            case State.Patrol:
                PatrolBehavior();
                break;

            case State.Flee:
                FleeBehavior();
                break;

            case State.Stunned:
                StunnedBehavior();
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("LadyBugOnly"))
        {
            if (debugLogs)
            {
                Debug.Log("[LadyBugAI] Hit edge wall, flipping direction");
            }
            ChangeState(State.Idle);
            Flip();
            // float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
            // TryReturnToPatrol(distanceToPlayer);
        }
    }

    private void ChangeState(State newState)
    {
        Debug.Log($"[LadyBugAI] State: {_currentState} → {newState}");
        _currentState = newState;
    }

    // TODO: Implementar métodos de comportamiento
    private void PatrolBehavior()
    {
        float direction = _movingRight ? 1 : -1;
        _rb.linearVelocity = new Vector2(direction * patrolSpeed, _rb.linearVelocity.y);

        float distanceFromStart = transform.position.x - _startPosition.x;

        if (_movingRight && distanceFromStart > patrolDistance)
        {
            Flip();
        }
        else if (!_movingRight && distanceFromStart < -patrolDistance)
        {
            Flip();
        }

        if (_player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

            if (distanceToPlayer < detectionRange)
            {
                if (debugLogs)
                {
                    Debug.Log($"[LadyBugAI] Player detected at distance {distanceToPlayer}, starting to flee!");
                }

                ChangeState(State.Flee);
            }
        }
    }

    private void FleeBehavior()
    {
        if (_player == null) return;

        Vector2 directiontoPlayer = (_player.position - transform.position).normalized;

        Vector2 fleeDirection = -directiontoPlayer;

        ApplyFleeVelocity(fleeDirection);

        FlipToFleeDirection(fleeDirection);

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        TryJumpOverPlayer(directiontoPlayer, distanceToPlayer);

        // TryReturnToPatrol(distanceToPlayer);
    }

    private void TryJumpOverPlayer(Vector2 directiontoPlayer, float distanceToPlayer)
    {
        if (distanceToPlayer < jumpOverDistance && _isGrounded)
        {
            bool isPlayerAhead =
                (directiontoPlayer.x > 0 && _movingRight) || (directiontoPlayer.x < 0 && !_movingRight);

            if (isPlayerAhead && Time.time >= _lastJumpTime + jumpCooldown)
            {
                if (debugLogs)
                {
                    Debug.Log("[LadyBugAI] Player blocking path, jumping over!");
                }

                JumpAhead(25);
            }
        }
    }

    private void FlipToFleeDirection(Vector2 fleeDirection)
    {
        if (fleeDirection.x > 0 && !_movingRight)
        {
            Flip();
        }
        else if (fleeDirection.x < 0 && _movingRight)
        {
            Flip();
        }
    }

    private void TryReturnToPatrol(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange * _fleeRangeHysteresis)
        {
            if (debugLogs)
            {
                Debug.Log($"[LadyBugAI] Player far enough ({distanceToPlayer}), returning to patrol");
            }

            ChangeState(State.Patrol);
        }
    }

    private void ApplyFleeVelocity(Vector2 fleeDirection)
    {
        _rb.linearVelocity = new Vector2(fleeDirection.x * fleeSpeed, _rb.linearVelocity.y);
    }

    private void StunnedBehavior()
    {
        if (Time.time >= _stunEndTime)
        {
            if (debugLogs)
            {
                Debug.Log($"[LadyBugAI] Stun ended, returning to {_previousState}");
            }

            ChangeState(_previousState);
        }
    }

    private void HandleKnockback(Vector2 direction, float damage)
    {
        if (_rb == null) return;

        // Guardar estado anterior
        if (_currentState != State.Stunned)
        {
            _previousState = _currentState;
        }

        // Aplicar knockback
        float knockbackForce = damage * 0.6f;
        _rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);

        // Cambiar a Stunned
        _stunEndTime = Time.time + stunDuration;
        ChangeState(State.Stunned);

        if (debugLogs)
        {
            Debug.Log($"[LadyBugAI] Knocked back! Stunned for {stunDuration}s");
        }
    }

    private void Jump()
    {
        if (_rb == null || _isGrounded == false) return;

        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        _lastJumpTime = Time.time;

        if (debugLogs)
        {
            Debug.Log("[LadyBugAI] Jumped!");
        }
    }
    
    private void JumpAhead(float jumpVelocityX)
    {
        if (_rb == null || _isGrounded == false) return;
        float jumpVelocity = _movingRight? jumpVelocityX : -jumpVelocityX;
        
        _rb.linearVelocity = new Vector2(jumpVelocity, jumpForce);
        _lastJumpTime = Time.time;

        if (debugLogs)
        {
            Debug.Log("[LadyBugAI] Jumped Ahead!");
        }
    }

    private void Flip()
    {
        _movingRight = !_movingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Área de patrulla
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_startPosition, patrolDistance);

        // Rango de detección (color según estado)
        Gizmos.color = _currentState == State.Flee ? Color.yellow : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Ground check self (para salto)
        if (groundCheckPoint != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadiusSelf);
        }

        // Jump over range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, jumpOverDistance);
    }
#endif
}