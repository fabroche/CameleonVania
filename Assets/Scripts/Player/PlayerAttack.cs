using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")] [SerializeField]
    private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackRange = 1.5f;
    private float _lastAttackTime = -999f;

    
    [Header("VFX")] [SerializeField] private ParticleSystem attackEffect;
    
    
    [Header("References")] [SerializeField]
    private Transform attackPoint;

    [SerializeField] private LayerMask enemyLayer;


    [SerializeField] private Animator animator;


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
    
    public void SetAttackDamage(float newDamage)
    {
        attackDamage = Mathf.Max(0f, newDamage); // Ensure non-negative
        Debug.Log($"[PlayerAttack] Attack damage set to: {attackDamage}");
    }
    
    public void SetAttackCooldown(float newCooldown)
    {
        attackCooldown = Mathf.Max(0.1f, newCooldown); // Minimum 0.1s to prevent spam
        Debug.Log($"[PlayerAttack] Attack cooldown set to: {attackCooldown}s");
    }
    
    public void SetAttackRange(float newRange)
    {
        attackRange = Mathf.Max(0.1f, newRange); // Minimum 1 to prevent 0 hitbox effect
        Debug.Log($"[PlayerAttack] Attack Range set to: {attackRange}");
    }
    
    public float GetAttackDamage() => attackDamage;
    
    public float GetAttackCooldown() => attackCooldown;

    public float GetAttackRange() => attackRange;
    
}