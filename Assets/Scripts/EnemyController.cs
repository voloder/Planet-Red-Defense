using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    enum State { MovingToBase, ShootingPlayer, ShootingBase }

    [Header("Base")]
    public Vector3 basePosition = new Vector3(-2, 0, 18);
    public float stoppingDistance = 2f;

    [Header("Base Attack")]
    public float baseAttackRange = 8f;

    State state;
    NavMeshAgent agent;
    Enemy enemy;
    Transform player;
    float shootTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
        {
            Debug.LogError("PlayerManager ili player nisu postavljeni u sceni!");
            enabled = false;
            return;
        }

        player = PlayerManager.instance.player.transform;
        agent.speed = enemy.moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        SetState(State.MovingToBase);
    }

    void Update()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        float distToBase = Vector3.Distance(transform.position, basePosition);

        bool playerAlive = PlayerHealth.instance == null || !PlayerHealth.instance.isDead;

        if (playerAlive && distToPlayer <= enemy.shootRange)
        {
            if (state != State.ShootingPlayer)
                SetState(State.ShootingPlayer);

            HandleShooting(player);
        }
        else if (distToBase <= baseAttackRange && BaseHealth.instance != null && BaseHealth.instance.IsAlive)
        {
            if (state != State.ShootingBase)
                SetState(State.ShootingBase);

            HandleShooting(BaseHealth.instance.transform);
        }
        else
        {
            if (state != State.MovingToBase)
                SetState(State.MovingToBase);
        }
    }

    void HandleShooting(Transform shootTarget)
    {
        FaceTarget(shootTarget.position);

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            Shoot(shootTarget);
            shootTimer = enemy.shootInterval;
        }
    }

    void SetState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.MovingToBase:
                agent.isStopped = false;
                if (agent.isOnNavMesh)
                    agent.SetDestination(basePosition);
                Debug.Log($"{name}: MovingToBase");
                break;

            case State.ShootingPlayer:
                agent.isStopped = true;
                shootTimer = 0f;
                Debug.Log($"{name}: ShootingPlayer");
                break;

            case State.ShootingBase:
                agent.isStopped = true;
                shootTimer = 0f;
                Debug.Log($"{name}: ShootingBase");
                break;
        }
    }

    void Shoot(Transform shootTarget)
    {
        if (enemy.bulletPrefab == null)
        {
            Debug.LogWarning($"{name}: bulletPrefab nije postavljen!");
            return;
        }

        Vector3 spawnPos = enemy.gunPoint != null
            ? enemy.gunPoint.position
            : transform.position + transform.forward * 0.5f;

        GameObject go = Instantiate(enemy.bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bullet = go.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Init(shootTarget, enemy.damage, enemy.bulletSpeed);
        else
            Debug.LogWarning("bulletPrefab nema Bullet komponentu!");
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8f);
    }

    private void OnDrawGizmosSelected()
    {
        Enemy e = GetComponent<Enemy>();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, e != null ? e.shootRange : 15f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(basePosition, baseAttackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(basePosition, stoppingDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, basePosition);
    }
}
