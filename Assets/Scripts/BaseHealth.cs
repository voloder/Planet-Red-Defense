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
        currentHealth = Mathf.Max(0f, currentHealth);
        if (amount > 0f)
            lastDamageTime = Time.time;

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        IsAlive = false;
        Debug.Log("BAZA JE UNISTENA!");
        GameOverController.Instance?.Show();
        Time.timeScale = 0f;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }
}
