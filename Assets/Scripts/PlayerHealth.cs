using System.Collections;
using UnityEngine;

public class PlayerHealth : Interactable
{
    public static PlayerHealth instance;

    [Header("Stats")]
    public float maxHealth = 100f;

    [Header("Respawn")]
    public Transform spawnPoint;

    float currentHealth;
    Vector3 startPosition;
    public bool isDead { get; private set; }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        startPosition = transform.position;
        currentHealth = maxHealth;
    }

    public override void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"Player HP: {currentHealth}/{maxHealth}");

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
        for (int i = 5; i > 0; i--)
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

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = spawnPoint != null ? spawnPoint.position : startPosition;
        if (cc != null) cc.enabled = true;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        Debug.Log("Player respawnovao!");
    }
}
