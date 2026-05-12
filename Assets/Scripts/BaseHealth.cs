using UnityEngine;

public class BaseHealth : Interactable
{
    public static BaseHealth instance;

    [Header("Stats")]
    public float maxHealth = 1000f;

    float currentHealth;
    public bool IsAlive { get; private set; } = true;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public override void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth -= amount;
        Debug.Log($"Baza HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        IsAlive = false;
        Debug.Log("Baza uništena! Game Over.");
        Destroy(gameObject);
    }
}
