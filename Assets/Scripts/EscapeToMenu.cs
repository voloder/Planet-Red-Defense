using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class EscapeToMenu : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;

    private VisualElement _root;
    private VisualElement _tutorialOverlay;
    private bool _menuOpen;

    void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.style.display = DisplayStyle.None;

        _tutorialOverlay = _root.Q<VisualElement>("tutorial-overlay");

        var startLabel = _root.Q<Button>("btn-start").Q<Label>();
        if (startLabel != null) startLabel.text = "RESUME";

        _root.Q<Button>("btn-start").clicked    += Resume;
        _root.Q<Button>("btn-tutorial").clicked += OnTutorial;
        _root.Q<Button>("btn-exit").clicked     += OnExit;
        _root.Q<Button>("btn-close").clicked    += OnClose;
    }

    void OnDisable()
    {
        _root.Q<Button>("btn-start").clicked    -= Resume;
        _root.Q<Button>("btn-tutorial").clicked -= OnTutorial;
        _root.Q<Button>("btn-exit").clicked     -= OnExit;
        _root.Q<Button>("btn-close").clicked    -= OnClose;

        SetPaused(false);
    }

    void Update()
    {
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (_menuOpen && !_tutorialOverlay.ClassListContains("hidden"))
            OnClose();
        else
            Toggle();
    }

    // ── Internals ────────────────────────────────────────────────────────────

    void Toggle()
    {
        _menuOpen = !_menuOpen;
        _root.style.display = _menuOpen ? DisplayStyle.Flex : DisplayStyle.None;
        SetPaused(_menuOpen);
    }

    void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;

        UnityEngine.Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        UnityEngine.Cursor.visible   = paused;

        if (playerShoot != null)
            playerShoot.enabled = !paused;
    }

    void Resume()
    {
        if (_menuOpen) Toggle();
    }

    void OnTutorial() => _tutorialOverlay.RemoveFromClassList("hidden");

    void OnClose()    => _tutorialOverlay.AddToClassList("hidden");

    void OnExit()
    {
        SetPaused(false);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
