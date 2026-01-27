using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")] [SerializeField]
    private float maxHealth = 100f;

    private float _currentHealth;

    public event System.Action OnDeath;
    public event System.Action<float> OnHealthChanged;

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        _currentHealth = Math.Clamp(_currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(_currentHealth);

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
}