using UnityEngine;

public class Enemy : Interactable
{
    [Header("Stats")] public float maxHealth = 100f;
    public float moveSpeed = 3.5f;
    public float damage = 10f;

    [Header("Shooting")] public float shootRange = 15f;
    public float shootInterval = 1.5f;
    public float bulletSpeed = 15f;
    public GameObject bulletPrefab;
    public Transform gunPoint;

    float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public override void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} primio {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        Debug.Log($"{name} je mrtav.");
        Destroy(gameObject);

        var copperAmount = Random.Range(5, 15);
        var ironAmount = Random.Range(5, 15);
        var titaniumAmount = Random.Range(0, 2);
        var diamondAmount = Random.Range(0, 10) == 0 ? 1 : 0;
        ResourceManager.Instance.AddResourcesWithFloatingText(copperAmount, ironAmount, titaniumAmount, diamondAmount, transform.position);
    }
}