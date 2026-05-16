using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameOverController : MonoBehaviour
{
    public static GameOverController Instance { get; private set; }

    VisualElement _panel;
    Label _goWave;
    bool _initialized;

    void Awake()
    {
        Instance = this;
    }

    void Init()
    {
        if (_initialized) return;
        _initialized = true;

        var root = GetComponent<UIDocument>().rootVisualElement;
        _panel  = root.Q<VisualElement>("game-over-panel");
        _goWave = root.Q<Label>("go-wave");

        if (_panel == null)
            Debug.LogError("[GameOverController] 'game-over-panel' nije pronađen u UXML!");

        root.Q<Button>("restart-btn")?.RegisterCallback<ClickEvent>(_ =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
        root.Q<Button>("exit-btn")?.RegisterCallback<ClickEvent>(_ => Application.Quit());

        _panel.style.display = DisplayStyle.None;
    }

    void Start()
    {
        Init();
    }

    public void Show()
    {
        Init();

        if (_panel == null)
        {
            Debug.LogError("[GameOverController] Panel je null, ne mogu prikazati Game Over ekran.");
            return;
        }

        int wave = WaveSpawner.Instance != null ? WaveSpawner.Instance.CurrentWave : 0;
        if (_goWave != null) _goWave.text = "WAVE REACHED: " + wave;
        _panel.style.display = DisplayStyle.Flex;
    }
}
