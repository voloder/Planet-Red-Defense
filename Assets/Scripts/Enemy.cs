using UnityEngine;
using TMPro;

public class Enemy : Interactable
{
    [Header("Stats")] public float maxHealth = 100f;
    public float moveSpeed = 3.5f;
    public float damage = 10f;

    [Header("Shooting")] public float shootRange = 15f;
    public float shootInterval = 1.5f;
    public Transform gunPoint;
    public bool useLineShot;

    [Header("Projectile Shot")]
    public float bulletSpeed = 15f;
    public GameObject bulletPrefab;

    [Header("Line Shot")]
    public GameObject linePrefab;
    public float lineDuration = 0.3f;
    public float lineMaxDistance = 100f;

    float currentHealth;

    [Header("Resource Drops")]
    public int copperMin = 5;
    public int copperMax = 12;
    public int ironMin = 3;
    public int ironMax = 8;
    public int titaniumMin = 0;
    public int titaniumMax = 0;
    [Range(0f, 1f)] public float diamondChance = 0f;

    [Header("Health Text (simple)")]
    public Vector3 healthBarOffset = new Vector3(0f, 2.2f, 0f);
    public int healthSegments = 10; // how many characters when at full health
    public GameObject healthTextPrefab; // assign a prefab that contains a configured TextMeshPro
    TextMeshPro healthText;

    void Start()
    {
        currentHealth = maxHealth;
        
        var go = Instantiate(healthTextPrefab);
        go.name = "HealthText";
        go.transform.SetParent(transform, false);
        go.transform.localPosition = healthBarOffset;
        go.transform.localRotation = Quaternion.identity;
        healthText = go.GetComponentInChildren<TextMeshPro>();

        UpdateHealthText();
    }

    public override void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);
        Debug.Log($"{name} primio {amount} damage. HP: {currentHealth}/{maxHealth}");

        UpdateHealthText();

        if (currentHealth <= 0f)
            Die();
    }

    void Update()
    {
        if (healthText != null && Camera.main != null)
        {
            Vector3 dir = healthText.transform.position - Camera.main.transform.position;
            if (dir.sqrMagnitude > 0.001f)
                healthText.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void UpdateHealthText()
    {
        float fract = currentHealth / maxHealth;
        int chars = Mathf.RoundToInt(fract * healthSegments);
        healthText.text = new string('I', chars);

        healthText.color = fract > 0.66 ? Color.green : fract > 0.3 ? Color.yellow : Color.red;
    }

    void Die()
    {
        Debug.Log($"{name} je mrtav.");
        Destroy(gameObject);

        var copperAmount = Random.Range(copperMin, copperMax + 1);
        var ironAmount = Random.Range(ironMin, ironMax + 1);
        var titaniumAmount = Random.Range(titaniumMin, titaniumMax + 1);
        var diamondAmount = Random.value < diamondChance ? 1 : 0;
        ResourceManager.Instance.AddResourcesWithFloatingText(copperAmount, ironAmount, titaniumAmount, diamondAmount, transform.position);
    }
}