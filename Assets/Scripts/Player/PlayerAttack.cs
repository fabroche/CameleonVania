using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")] [SerializeField]
    private float attackDamage = 20f;

    [Header("VFX")] [SerializeField] private ParticleSystem attackEffect;
    
    [SerializeField] private float attackRange = 1.5f;

    [Header("References")] [SerializeField]
    private Transform attackPoint;

    [SerializeField] private LayerMask enemyLayer;

    [SerializeField] private float attackCooldown = 0.5f;

    [SerializeField] private Animator animator;

    private float _lastAttackTime = -999f;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Time.time >= _lastAttackTime + attackCooldown)
            {
                Attack();
                _lastAttackTime = Time.time;
            }
            else
            {
                // Opcional: Debug para ver cuánto falta
                float timeLeft = (_lastAttackTime + attackCooldown) - Time.time;
                Debug.Log($"Cooldown! Espera {timeLeft:F2}s");
            }
        }
    }

    private void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (attackEffect != null)
        {
            attackEffect.Play();
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        Debug.Log($"Atacando! Enemigos detectados: {hitEnemies.Length}");

        foreach (Collider2D enemy in hitEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();

            Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
            
            enemyHealth?.TakeDamage(attackDamage, knockbackDirection);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}