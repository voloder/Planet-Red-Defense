using System;
using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    [Header("Craft")]
    [SerializeField] private float craftDuration = 3f;

    // wire up from AmmoSystem: OnCraftComplete += () => AddAmmo(30);
    public Action OnCraftComplete;

    private Label _waveNumber;
    private Label _waveTopLabel;
    private VisualElement _baseAttackWarning;
    private bool _isBaseWarningVisible;

    private Label _ironValue;
    private Label _copperValue;
    private Label _titaniumValue;
    private Label _diamondValue;

    private Label _ammoCurrent;
    private Label _ammoMax;
    private VisualElement _ammoBarFill;

    private VisualElement _craftBtnFill;
    private VisualElement _craftProgressRoot;
    private VisualElement _craftBarFill;

    private bool _isCrafting;
    private float _craftElapsed;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _waveNumber    = root.Q<Label>("wave-number");
        _waveTopLabel  = root.Q<Label>("wave-top-label");
        _baseAttackWarning = root.Q<VisualElement>("BaseAttackWarning");

        if (_baseAttackWarning != null)
            _baseAttackWarning.style.display = DisplayStyle.None;

        _ironValue     = root.Q<Label>("iron-value");
        _copperValue   = root.Q<Label>("copper-value");
        _titaniumValue = root.Q<Label>("titanium-value");
        _diamondValue  = root.Q<Label>("diamond-value");

        _ammoCurrent = root.Q<Label>("ammo-current");
        _ammoMax     = root.Q<Label>("ammo-max");
        _ammoBarFill = root.Q<VisualElement>("ammo-bar-fill");
    }


    void Update()
    {
        UpdateBaseAttackWarning();

#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame)
            StartCraft();
#endif

        if (!_isCrafting) return;

        _craftElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_craftElapsed / craftDuration);
        _craftBtnFill.style.width = Length.Percent(t * 100f);

        if (t >= 1f)
            CompleteCraft();
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public void SetWave(int wave) =>
        _waveNumber.text = wave.ToString("D2");

    public void SetWavePhase(int wave, bool isBreak, int secsLeft)
    {
        if (isBreak)
        {
            if (_waveTopLabel != null) _waveTopLabel.text = wave == 0 ? "PREPARE" : "WAVE CLEARED";
            _waveNumber.text = secsLeft + "s";
        }
        else
        {
            if (_waveTopLabel != null) _waveTopLabel.text = "SIEGE WAVE";
            _waveNumber.text = wave.ToString("D2");
        }
    }

    public void SetResources(int iron, int copper, int titanium, int diamond)
    {
        _ironValue.text     = iron.ToString();
        _copperValue.text   = copper.ToString();
        _titaniumValue.text = titanium.ToString();
        _diamondValue.text  = diamond.ToString();
    }

    public void SetAmmo(int current, int max)
    {
        _ammoCurrent.text = current.ToString();
        _ammoMax.text     = max.ToString();
        float pct = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        _ammoBarFill.style.width = Length.Percent(pct * 100f);
    }

    public void ShowCraftProgress(float t)
    {
        _craftProgressRoot.RemoveFromClassList("hidden");
        _craftBarFill.style.width = Length.Percent(Mathf.Clamp01(t) * 100f);
    }

    public void HideCraftProgress()
    {
        _craftProgressRoot.AddToClassList("hidden");
    }

    // call on wave start, player death, or any interrupt
    public void CancelCraft()
    {
        if (!_isCrafting) return;
        _isCrafting = false;
        _craftElapsed = 0f;
        _craftBtnFill.style.width = Length.Percent(0f);
    }

    // ── Craft internal ───────────────────────────────────────────────────────

    [ContextMenu("Test: Start Craft")]
    private void StartCraft()
    {
        if (_isCrafting) return;
        _isCrafting = true;
        _craftElapsed = 0f;
    }

    private void CompleteCraft()
    {
        _isCrafting = false;
        _craftBtnFill.style.width = Length.Percent(0f);
        OnCraftComplete?.Invoke();
    }

    private void UpdateBaseAttackWarning()
    {
        if (_baseAttackWarning == null)
            return;

        bool shouldShow = BaseHealth.instance != null && BaseHealth.instance.IsUnderAttack;
        if (shouldShow == _isBaseWarningVisible)
            return;

        _isBaseWarningVisible = shouldShow;
        _baseAttackWarning.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
