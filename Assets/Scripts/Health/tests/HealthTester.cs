using System;
using UnityEngine;

public class HealthTester : MonoBehaviour
{
    [SerializeField] private Health health;

    private void Start()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        health.OnHealthChanged += OnHealthChanged;
        health.OnDeath += OnDeath;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            health.TakeDamage(20f, Vector2.zero);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            health.Heal(15f);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            health.TakeDamage(1000f, Vector2.zero);
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= OnHealthChanged;
            health.OnDeath -= OnDeath;
        }
    }

    private void OnHealthChanged(float newHealth)
    {
        Debug.Log($"[EVENT] Health changed: {newHealth}");
    }

    private void OnDeath()
    {
        Debug.Log($"[EVENT] {gameObject.name} died!");
    }
}