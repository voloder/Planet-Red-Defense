using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    enum State { MovingToBase, ShootingPlayer, ShootingBase }

    static readonly int WalkStateHash = Animator.StringToHash("Walk");
    static readonly int AttackStateHash = Animator.StringToHash("Attack");

    [Header("Base")]
    public Vector3 basePosition = new Vector3(-2, 0, 18);
    public float stoppingDistance = 2f;

    [Header("Base Attack")]
    public float baseAttackRange = 8f;

    [Header("Animation")]
    public Animator animator;
    public string walkStateName = "Walk_F";
    public string attackStateName = "Attack1";
    public float animationCrossFade = 0.1f;

    State _state;
    NavMeshAgent _agent;
    Enemy _enemy;
    Transform _player;
    float _shootTimer;
    
    public AudioClip shootClip;
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemy = GetComponent<Enemy>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
        {
            Debug.LogError("PlayerManager ili player nisu postavljeni u sceni!");
            enabled = false;
            return;
        }

        _player = PlayerManager.instance.player.transform;
        _agent.speed = _enemy.moveSpeed;
        _agent.stoppingDistance = stoppingDistance;

        SetState(State.MovingToBase);
    }

    void Update()
    {
        float distToPlayer = Vector3.Distance(transform.position, _player.position);
        float distToBase = Vector3.Distance(transform.position, basePosition);

        bool playerAlive = PlayerHealth.instance == null || !PlayerHealth.instance.isDead;

        if (playerAlive && distToPlayer <= _enemy.shootRange)
        {
            if (_state != State.ShootingPlayer)
                SetState(State.ShootingPlayer);

            HandleShooting(_player);
        }
        else if (distToBase <= baseAttackRange && BaseHealth.instance != null && BaseHealth.instance.IsAlive)
        {
            if (_state != State.ShootingBase)
                SetState(State.ShootingBase);

            HandleShooting(BaseHealth.instance.transform);
        }
        else
        {
            if (_state != State.MovingToBase)
                SetState(State.MovingToBase);
        }
    }

    void HandleShooting(Transform shootTarget)
    {
        FaceTarget(shootTarget.position);

        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0f)
        {
            Shoot(shootTarget);
            _shootTimer = _enemy.shootInterval;
        }
    }

    void SetState(State newState)
    {
        _state = newState;

        switch (newState)
        {
            case State.MovingToBase:
                _agent.isStopped = false;
                if (_agent.isOnNavMesh)
                    _agent.SetDestination(basePosition);
                PlayAnimation(walkStateName, WalkStateHash);
                Debug.Log($"{name}: MovingToBase");
                break;

            case State.ShootingPlayer:
                _agent.isStopped = true;
                _shootTimer = 0f;
                PlayAnimation(attackStateName, AttackStateHash);
                Debug.Log($"{name}: ShootingPlayer");
                break;

            case State.ShootingBase:
                _agent.isStopped = true;
                _shootTimer = 0f;
                PlayAnimation(attackStateName, AttackStateHash);
                Debug.Log($"{name}: ShootingBase");
                break;
        }
    }

    void PlayAnimation(string stateName, int fallbackHash)
    {
        if (animator == null)
            return;

        if (string.IsNullOrWhiteSpace(stateName))
            animator.CrossFadeInFixedTime(fallbackHash, animationCrossFade);
        else
            animator.CrossFadeInFixedTime(stateName, animationCrossFade);
    }

    void Shoot(Transform shootTarget)
    {
        if (_enemy.useLineShot)
        {
            ShootLine(shootTarget);
            return;
        }

        if (_enemy.bulletPrefab == null)
        {
            Debug.LogWarning($"{name}: bulletPrefab nije postavljen!");
            return;
        }

        Vector3 spawnPos = _enemy.gunPoint != null
            ? _enemy.gunPoint.position
            : transform.position + transform.forward * 0.5f;

        GameObject go = Instantiate(_enemy.bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bullet = go.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Init(shootTarget, _enemy.damage, _enemy.bulletSpeed);
        else
            Debug.LogWarning("bulletPrefab nema Bullet komponentu!");

        // Play shooting sound
        if (shootClip != null)
        {
            Camera.main.GetComponent<AudioSource>().PlayOneShot(shootClip);
        }
    }

    void ShootLine(Transform shootTarget)
    {
        Vector3 origin = _enemy.gunPoint != null
            ? _enemy.gunPoint.position
            : transform.position + transform.forward * 0.5f;

        Vector3 targetPoint = shootTarget.position;
        float maxDistance = Mathf.Max(0.01f, _enemy.lineMaxDistance);
        Vector3 direction = targetPoint - origin;

        if (direction.sqrMagnitude > maxDistance * maxDistance)
            targetPoint = origin + direction.normalized * maxDistance;

        if (_enemy.linePrefab != null)
        {
            GameObject lineObj = Instantiate(_enemy.linePrefab);
            LineRenderer lr = lineObj.GetComponent<LineRenderer>();

            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, origin);
                lr.SetPosition(1, targetPoint);
            }
            else
            {
                Debug.LogWarning($"{name}: linePrefab nema LineRenderer komponentu!");
            }

            Destroy(lineObj, _enemy.lineDuration);
        }
        else
        {
            Debug.LogWarning($"{name}: linePrefab nije postavljen!");
        }

        if (shootClip != null)
        {
            Camera.main.GetComponent<AudioSource>().PlayOneShot(shootClip);
        }

        Interactable interactable = shootTarget.GetComponent<Interactable>();
        if (interactable != null)
            interactable.TakeDamage(_enemy.damage);
        else
            Debug.LogWarning($"{name}: {shootTarget.name} nema Interactable komponentu.");
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
