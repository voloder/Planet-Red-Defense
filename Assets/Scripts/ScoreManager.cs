using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    int _kills;
    HUDController _hud;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _hud = Object.FindFirstObjectByType<HUDController>();
    }

    public void AddKill()
    {
        _kills++;
    }

    public void TriggerGameOver()
    {
        int wave = WaveSpawner.Instance != null ? WaveSpawner.Instance.CurrentWave : 0;
        int score = _kills * 10 + wave * 100;
        GameOverController.Instance?.Show();
        Time.timeScale = 0f;
    }

    public static void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
