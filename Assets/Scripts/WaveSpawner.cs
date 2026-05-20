using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance { get; private set; }

    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Enemy Prefabs")]
    public GameObject tier1Prefab; // GraveStalker
    public GameObject tier2Prefab; // robot_metallic
    public GameObject tier3Prefab; // Drone
    public GameObject bossPrefab;  // Boss

    [Header("Timing")]
    [SerializeField] float spawnInterval = 0.8f;
    [SerializeField] float breakDuration = 15f;

    public int CurrentWave => _currentWave;
    int _currentWave = 0;
    int _aliveCount = 0;
    float _breakTimer;
    HUDController _hud;

    enum State { Break, Spawning, WaitingForKills }
    State _state;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _hud = Object.FindFirstObjectByType<HUDController>();
        _breakTimer = 5f;
        _state = State.Break;
    }

    void Update()
    {
        switch (_state)
        {
            case State.Break:
                _breakTimer -= Time.deltaTime;
                _hud?.SetWavePhase(_currentWave, true, Mathf.CeilToInt(_breakTimer));
                if (_breakTimer <= 0f)
                    StartNextWave();
                break;

            case State.WaitingForKills:
                if (_aliveCount <= 0)
                    EnterBreak();
                break;
        }
    }

    void StartNextWave()
    {
        _currentWave++;
        _hud?.SetWavePhase(_currentWave, false, 0);
        StartCoroutine(SpawnWave(_currentWave));
        _state = State.Spawning;
    }

    IEnumerator SpawnWave(int wave)
    {
        float healthMult = 1f + (wave - 1) * 0.15f;
        var enemies = BuildEnemyList(wave, healthMult);

        foreach (var (prefab, mult) in enemies)
        {
            var go = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            var enemy = go.GetComponent<Enemy>();
            if (enemy != null)
                enemy.spawnHealthMultiplier = mult;
            _aliveCount++;
            yield return new WaitForSeconds(spawnInterval);
        }

        _state = State.WaitingForKills;
    }

    List<(GameObject prefab, float healthMult)> BuildEnemyList(int wave, float healthMult)
    {
        int t1 = wave <= 3 ? (2 + wave) : Mathf.Max(2, 6 - wave);
        int t2 = wave >= 4 ? Mathf.Min(wave - 3, 8) : 0;
        int t3 = wave >= 7 ? Mathf.Min(wave - 6, 6) : 0;
        int boss = wave >= 10 ? wave / 5 - 1 : 0;

        var list = new List<(GameObject, float)>();

        for (int i = 0; i < t1; i++) list.Add((tier1Prefab, healthMult));
        for (int i = 0; i < t2; i++) list.Add((tier2Prefab, healthMult));
        for (int i = 0; i < t3; i++) list.Add((tier3Prefab, healthMult));
        for (int i = 0; i < boss; i++) list.Add((bossPrefab, healthMult));
        
        ShuffleList(list);

        return list;
    }

    void ShuffleList(List<(GameObject, float)> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    void EnterBreak()
    {
        _breakTimer = breakDuration;
        _state = State.Break;
    }

    public void OnEnemyDied()
    {
        _aliveCount = Mathf.Max(0, _aliveCount - 1);
    }
}
