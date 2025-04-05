using UnityEngine;
using System;

public class TestDummy : MonoBehaviour, ITargetable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health;
    [SerializeField] private bool autoRespawn = true;
    [SerializeField] private float respawnDelay = 0.5f;

    public event Action<float, bool, int, float> OnDamaged;
    public event Action OnDeath;

    private Vector3 initialPosition;
    private bool isDead = false;
    private float respawnTimer = 0f;

    public Transform Transform => transform;
    public bool IsAlive => health > 0;
    public Vector3 Position => transform.position;

    private void Awake()
    {
        initialPosition = transform.position;
        health = maxHealth;
    }

    private void Update()
    {
        if (isDead && autoRespawn)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnDelay)
            {
                Respawn();
            }
        }
    }

    public void Setup(float newMaxHealth, bool enableAutoRespawn)
    {
        maxHealth = newMaxHealth;
        health = maxHealth;
        autoRespawn = enableAutoRespawn;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        // Calculate overkill amount
        float overkillAmount = 0;
        if (damage > health)
        {
            overkillAmount = damage - health;
        }

        health -= damage;

        // Track metrics
        OnDamaged?.Invoke(damage, true, 1, overkillAmount);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        health = 0;

        // Hide renderer but keep object alive
        if (GetComponent<Renderer>())
        {
            GetComponent<Renderer>().enabled = false;
        }

        OnDeath?.Invoke();

        if (autoRespawn)
        {
            respawnTimer = 0f;
        }
    }

    private void Respawn()
    {
        isDead = false;
        health = maxHealth;

        // Show renderer
        if (GetComponent<Renderer>())
        {
            GetComponent<Renderer>().enabled = true;
        }
    }

    public void Reset()
    {
        transform.position = initialPosition;
        health = maxHealth;
        isDead = false;
        respawnTimer = 0f;

        // Show renderer
        if (GetComponent<Renderer>())
        {
            GetComponent<Renderer>().enabled = true;
        }
    }
}