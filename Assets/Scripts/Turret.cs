using UnityEngine;

public class Turret : MonoBehaviour
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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootClip;

    readonly Collider[] _overlapHits = new Collider[64];
    Enemy _currentTarget;
    float _nextFireTime;

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
            if (audioSource != null)
                audioSource.PlayOneShot(shootClip);
            else
                AudioSource.PlayClipAtPoint(shootClip, origin.position);
        }


            _currentTarget.TakeDamage(damage);

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
