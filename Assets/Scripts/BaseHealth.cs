using UnityEngine;

public class BaseHealth : Interactable
{
    public static BaseHealth instance;

    [Header("Stats")]
    public float maxHealth = 1000f;
    [SerializeField] private float underAttackWarningDuration = 1.5f;

    float currentHealth;
    float lastDamageTime = float.NegativeInfinity;
    public bool IsAlive { get; private set; } = true;
    public bool IsUnderAttack => IsAlive && Time.time - lastDamageTime <= underAttackWarningDuration;

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
        if (amount > 0f)
            lastDamageTime = Time.time;
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
