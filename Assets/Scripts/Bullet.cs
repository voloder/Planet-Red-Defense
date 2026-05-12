using UnityEngine;

public class Bullet : MonoBehaviour
{
    Transform target;
    float damage;
    float speed;
    bool hasHit;

    public void Init(Transform target, float damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;

        GetComponent<Renderer>().material.color = Color.yellow;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(transform.position, target.position) < 0.3f)
            Hit();
    }

    void OnTriggerEnter(Collider other)
    {
        if (target != null && other.gameObject == target.gameObject)
            Hit();
    }

    void Hit()
    {
        if (hasHit) return;
        hasHit = true;

        Interactable interactable = target.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.TakeDamage(damage);
            Debug.Log($"Bullet pogodio {target.name} za {damage} damage.");
        }
        else
        {
            Debug.LogWarning($"{target.name} nema Interactable komponentu — damage nije nanesen.");
        }

        Destroy(gameObject);
    }
}
