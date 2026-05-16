using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerHealth : Interactable
{
    public static PlayerHealth instance;

    [Header("Stats")]
    public float maxHealth = 100f;

    [Header("Health Bar")]
    public GameObject healthTextPrefab;
    public Vector3 healthBarOffset = new Vector3(0f, 2.5f, 0f);
    public int healthSegments = 10;

    [Header("Respawn")]
    public Transform spawnPoint;

    float currentHealth;
    Vector3 startPosition;
    public bool isDead { get; private set; }
    TextMeshPro _healthText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        startPosition = transform.position;
        currentHealth = maxHealth;

        if (healthTextPrefab != null)
        {
            var go = Instantiate(healthTextPrefab);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = healthBarOffset;
            _healthText = go.GetComponentInChildren<TextMeshPro>();
            UpdateHealthText();
        }
    }

    void Update()
    {
        if (_healthText != null && Camera.main != null)
        {
            Vector3 dir = _healthText.transform.position - Camera.main.transform.position;
            if (dir.sqrMagnitude > 0.001f)
                _healthText.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void UpdateHealthText()
    {
        if (_healthText == null) return;
        float fract = currentHealth / maxHealth;
        int chars = Mathf.RoundToInt(fract * healthSegments);
        _healthText.text = new string('I', chars);
        _healthText.color = fract > 0.66f ? Color.green : fract > 0.3f ? Color.yellow : Color.red;
    }

    public override void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);
        UpdateHealthText();

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        isDead = true;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        Debug.Log("Player umro!");
        StartCoroutine(RespawnCountdown());
    }

    IEnumerator RespawnCountdown()
    {
        for (int i = 3; i > 0; i--)
        {
            Debug.Log($"Respawn za {i}...");
            yield return new WaitForSeconds(1f);
        }

        if (BaseHealth.instance != null && BaseHealth.instance.IsAlive)
            Respawn();
        else
            Debug.Log("Baza je uništena — Game Over.");
    }

    void Respawn()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthText();

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = spawnPoint != null ? spawnPoint.position : startPosition;
        if (cc != null) cc.enabled = true;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        Debug.Log("Player respawnovao!");
    }
}
