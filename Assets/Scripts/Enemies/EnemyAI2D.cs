using System;
using UnityEngine;

public class EnemyAI2D : MonoBehaviour
{
    [SerializeField]
    private enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Stunned
    };

    private State _currentState;

    [Header("Movement")] [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool _movingRight = true;

    [Header("Detection")] [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;
    private float _chaseRangeHysteresis = 1.5f;

    [Header("Combat")] [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float _attackRangeHysteresis = 1.2f;

    [Header("Knockback/Stun")]
    [SerializeField] private float stunDuration = 0.5f;
    private float stunEndTime = 0f;
    private State previousState = State.Patrol; // State to return to after stun

    [Header("References")] private Rigidbody2D _rb;
    private Transform _player;
    private Vector2 _startPosition;
    private float _lastAttackTime;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure Player has 'Player' tag.");
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

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnTakeDamageWithKnockback -= HandleKnockback;
        }
    }

    private void HandleKnockback(Vector2 direction, float damage)
    {
        // Knockback force proportional to damage
        float knockbackForce = damage * 0.6f; // 15 damage = 9 force
        ApplyKnockback(direction, knockbackForce);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Idle:
                Debug.Log("[EnemyAI] Idle");
                break;

            case State.Patrol:
                PatrolBehavior();
                break;

            case State.Chase:
                ChaseBehavior();
                break;

            case State.Attack:
                AttackBehavior();
                break;

            case State.Stunned:
                StunnedBehavior();
                break;
        }
    }

    /// <summary>
    /// Stunned state - wait until stun duration ends
    /// </summary>
    private void StunnedBehavior()
    {
        // Stop movement
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocityY);

        // Check if stun duration has ended
        if (Time.time >= stunEndTime)
        {
            Debug.Log($"[EnemyAI] Stun ended, returning to {previousState}");
            ChangeState(previousState);
        }
    }

    private void ChangeState(State newState)
    {
        Debug.Log($"[EnemyAI] Changing state: {_currentState} → {newState}");
        _currentState = newState;
    }

    private void PatrolBehavior()
    {
        float direction = _movingRight ? 1 : -1;
        _rb.linearVelocity = new Vector2(direction * patrolSpeed, _rb.linearVelocityY);

        float distanceFromStart = transform.position.x - _startPosition.x;

        if (_movingRight && distanceFromStart > patrolDistance)
        {
            Flip();
        }
        else if (!_movingRight && distanceFromStart < -patrolDistance)
        {
            Flip();
        }

        CheckForPlayerDistance();
    }

    private void CheckForPlayerDistance()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer < detectionRange)
        {
            ChangeState(State.Chase);
        }
    }

    private void ChaseBehavior()
    {
        if (_player == null) return;

        Vector2 direction = (_player.position - transform.position).normalized;

        _rb.linearVelocity = new Vector2(direction.x * chaseSpeed, _rb.linearVelocityY);

        if (direction.x > 0 && !_movingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && _movingRight)
        {
            Flip();
        }

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer < detectionRange)
        {
            ChangeState(State.Chase);

            if (distanceToPlayer < attackRange)
            {
                ChangeState(State.Attack);
            }
            
        }
        else if (distanceToPlayer > detectionRange * _chaseRangeHysteresis)
        {
            ChangeState(State.Patrol);
        }
    }

    private void AttackBehavior()
    {
        if (_player == null) return;

        _rb.linearVelocity = new Vector2(0, _rb.linearVelocityY);

        if (Time.time >= _lastAttackTime + attackCooldown)
        {
            Health playerHealth = _player.GetComponent<Health>();
            Vector2 knockBackDirection = (transform.position - _player.position).normalized;
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage, knockBackDirection);
                _lastAttackTime = Time.time;
                Debug.Log($"[EnemyAI] Attacked player for {attackDamage} damage!");
            }
            else
            {
                Debug.LogWarning("[EnemyAI] Player doesn't have Health component!");
            }
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer > attackRange * _attackRangeHysteresis)
        {
            ChangeState(State.Chase);
        }
    }

    /// <summary>
    /// Apply knockback force and stun the enemy temporarily
    /// Called when enemy takes damage
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (_rb == null) return;

        // Save current state to return to after stun
        if (_currentState != State.Stunned)
        {
            previousState = _currentState;
        }

        // Apply knockback force
        _rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

        // Change to Stunned state
        stunEndTime = Time.time + stunDuration;
        ChangeState(State.Stunned);

        Debug.Log($"[EnemyAI] Knocked back! Stunned for {stunDuration}s (will return to {previousState})");
    }

    private void Flip()
    {
        _movingRight = !_movingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.dodgerBlue;
        Gizmos.DrawWireSphere(_startPosition, patrolDistance);

        // Detection range - changes color based on state
        if (_currentState == State.Stunned)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange for stunned
        }
        else if (_currentState == State.Chase)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = _currentState == State.Attack ? Color.darkRed : Color.indigo;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}