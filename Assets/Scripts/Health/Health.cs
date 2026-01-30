using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")] [SerializeField]
    private float maxHealth = 100f;

    private float _currentHealth;

    public event System.Action OnDeath;
    public event System.Action<float> OnHealthChanged;
    public event System.Action<Vector2, float> OnTakeDamageWithKnockback;

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce = 15f)
    {
        _currentHealth -= damage;

        _currentHealth = Math.Clamp(_currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(_currentHealth);

        // Notify subscribers about knockback (they will handle the actual force application)
        OnTakeDamageWithKnockback?.Invoke(knockbackDirection, damage);
        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {_currentHealth}/{maxHealth}");

        if (_currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        _currentHealth += amount;

        _currentHealth = Math.Clamp(_currentHealth, 0f, maxHealth);

        Debug.Log($"{gameObject.name} healed {amount}. Health: {_currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(_currentHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died!");

        OnDeath?.Invoke();

        Destroy(gameObject, 0.5f);
    }

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsAlive()
    {
        return _currentHealth > 0;
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = Mathf.Max(1f, newMaxHealth); // Minimum 1 HP
        
        // Adjust current health if it exceeds new max
        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
            OnHealthChanged?.Invoke(_currentHealth);
        }
        
        Debug.Log($"[Health] Max health set to: {maxHealth}");
    }
}