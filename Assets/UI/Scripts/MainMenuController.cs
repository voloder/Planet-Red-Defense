using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";

    private VisualElement _tutorialOverlay;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _tutorialOverlay = root.Q<VisualElement>("tutorial-overlay");

        root.Q<Button>("btn-start").clicked    += OnStart;
        root.Q<Button>("btn-tutorial").clicked += OnTutorial;
        root.Q<Button>("btn-exit").clicked     += OnExit;
        root.Q<Button>("btn-close").clicked    += OnClose;
    }

    void OnDisable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Button>("btn-start").clicked    -= OnStart;
        root.Q<Button>("btn-tutorial").clicked -= OnTutorial;
        root.Q<Button>("btn-exit").clicked     -= OnExit;
        root.Q<Button>("btn-close").clicked    -= OnClose;
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            OnClose();
    }

    private void OnStart()    => SceneManager.LoadScene(gameSceneName);
    private void OnTutorial() => _tutorialOverlay.RemoveFromClassList("hidden");
    private void OnClose()    => _tutorialOverlay.AddToClassList("hidden");
    private void OnExit()     => Application.Quit();
}
