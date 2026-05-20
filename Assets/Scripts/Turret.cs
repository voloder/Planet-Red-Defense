using Mono.Cecil;
using UnityEngine;
using TMPro;

public class Turret : Interactable
{
    [Header("Targeting")]
    public float range = 15f;

    [Header("Shooting")]
    public Transform rotationPivot;
    public Transform firePoint;
    public GameObject linePrefab;
    public float maxDistance = 100f;
    public float lineDuration = 0.3f;
    public float fireRate = 2f;
    public float damage = 10f;

    [Header("Health")]
    public float maxHealth = 50f;

    [Header("Health Display")]
    public Vector3 healthBarOffset = new Vector3(0f, 2.2f, 0f);
    public int healthSegments = 10;
    public GameObject healthTextPrefab;

    [Header("Audio")]
    public AudioClip shootClip;

    readonly Collider[] _overlapHits = new Collider[64];
    Enemy _currentTarget;
    float _nextFireTime;
    float _currentHealth;
    TextMeshPro _healthText;
    
    void Start()
    {
        _currentHealth = maxHealth;
        
        if (healthTextPrefab != null)
        {
            GameObject go = Instantiate(healthTextPrefab);
            go.name = "HealthText";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = healthBarOffset;
            go.transform.localRotation = Quaternion.identity;
            _healthText = go.GetComponentInChildren<TextMeshPro>();
            
            UpdateHealthText();
        }
    }
    
    void Update()
    {
        AcquireTarget();

        if (_currentTarget == null)
            return;

        transform.LookAt(new Vector3(_currentTarget.transform.position.x, transform.position.y, _currentTarget.transform.position.z));

        if (Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        }
        
        // Update health text billboard rotation
        if (_healthText != null && Camera.main != null)
        {
            Vector3 dir = _healthText.transform.position - Camera.main.transform.position;
            if (dir.sqrMagnitude > 0.001f)
                _healthText.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void AcquireTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, range, _overlapHits);
        float bestDistance = float.MaxValue;
        Enemy bestTarget = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _overlapHits[i];
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy == null)
                continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = enemy;
            }
        }

        _currentTarget = bestTarget;
    }


    void Shoot()
    {
        Transform origin = firePoint != null ? firePoint : (rotationPivot != null ? rotationPivot : transform);
        Vector3 direction = (_currentTarget.transform.position - origin.position).normalized;
        direction.Normalize();

        Vector3 finalHitPoint = origin.position + direction * maxDistance;

        if (linePrefab != null)
        {
            GameObject lineObj = Instantiate(linePrefab);
            LineRenderer lr = lineObj.GetComponent<LineRenderer>();

            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, origin.position);
                lr.SetPosition(1, finalHitPoint);
            }
            else
            {
                Debug.LogWarning($"{name}: linePrefab does not have a LineRenderer component.");
            }

            Destroy(lineObj, lineDuration);
        }
        else
        {
            Debug.LogWarning($"{name}: linePrefab is not assigned.");
        }

        if (shootClip != null)
        { 
            Camera.main.GetComponent<AudioSource>().PlayOneShot(shootClip);
        }


            _currentTarget.TakeDamage(damage);

    }

    public override void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Max(0f, _currentHealth);
        Debug.Log($"{name} received {amount} damage. HP: {_currentHealth}/{maxHealth}");

        UpdateHealthText();

        if (_currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        Debug.Log($"{name} has been destroyed.");
        Destroy(gameObject);
    }

    void UpdateHealthText()
    {
        if (_healthText == null)
            return;

        float fract = _currentHealth / maxHealth;
        int chars = Mathf.RoundToInt(fract * healthSegments);
        _healthText.text = new string('I', chars);

        _healthText.color = fract > 0.66f ? Color.green : fract > 0.3f ? Color.yellow : Color.red;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    public void Repair()
    {
        if (_currentHealth < maxHealth && ResourceManager.Instance.CanUseResources(1, 1, 0, 0))
        {
            ResourceManager.Instance.UseResources(1, 1, 0, 0);
            _currentHealth += 5;
            UpdateHealthText();
        }
    }
}
