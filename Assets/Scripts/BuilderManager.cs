using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuilderManager : MonoBehaviour
{
    public static BuilderManager Instance { get; private set; }

    [Header("Build Settings")]
    [SerializeField] private LayerMask buildSurfaceMask;
    [SerializeField] private float maxBuildDistance = 100f;

    [System.Serializable]
    public class BuildOption
    {
        public string optionName;
        public Key key;
        public GameObject prefab;
        public bool isDrill;
        public GameObject hologramPrefab;
        public GameObject redHologramPrefab;
    }

    [Header("Build Options")]
    [SerializeField] private List<BuildOption> buildOptions = new List<BuildOption>();

    [Header("Hologram (default)")]
    [SerializeField] private GameObject hologramPrefab;
    [SerializeField] private GameObject redHologramPrefab;
    
    [Header("Terrain")]
    [SerializeField] private Terrain terrain;

    
    [Header("Cancel key")]
    public Key cancelKey = Key.Q;
    
    private bool _isPlacing;
    private bool _canPlaceOnCurrentTarget;
    private bool _currentHologramIsRed;
    private BuildOption _currentBuildOption;

    private GameObject _currentHologram;
    private GameObject _currentHologramPrefab;
    private Camera _cam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            // Check each configured build option's key to start placing that option
            foreach (var opt in buildOptions)
            {
                if (Keyboard.current[opt.key].wasPressedThisFrame)
                {
                    _isPlacing = true;
                    _currentBuildOption = opt;
                    break;
                }
            }

            if (Keyboard.current[cancelKey].wasPressedThisFrame)
            {
                _isPlacing = false;
                _currentBuildOption = null;
            }
        }
        
        HandleRaycast();
        HandlePlacement();
    }

    private void HandleRaycast()
    {
        if(!_isPlacing)
        {
            _canPlaceOnCurrentTarget = false;
            HideHologram();
            return;
        }
        if (Mouse.current == null || _cam == null)
        {
            _canPlaceOnCurrentTarget = false;
            HideHologram();
            return;
        }

        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, buildSurfaceMask))
        {
            if (_currentBuildOption == null)
            {
                _canPlaceOnCurrentTarget = false;
                HideHologram();
                return;
            }

            // Determine if the hit target (or its parents) is one of the ore tags
            bool isOre = HasTagInParents(hit.collider.transform, "CopperOre")
                         || HasTagInParents(hit.collider.transform, "IronOre")
                         || HasTagInParents(hit.collider.transform, "TitaniumOre");

            // If this build option is a drill, it can only be placed on ore.
            // Otherwise placement is allowed anywhere on the build surface.
            bool canPlace = _currentBuildOption.isDrill ? isOre : true;

            _canPlaceOnCurrentTarget = canPlace;
            ShowHologram(hit, canPlace);
        }
        else
        {
            _canPlaceOnCurrentTarget = false;
            HideHologram();
        }
    }


    private void ShowHologram(RaycastHit hit, bool isValidPlacement)
    {
        bool shouldUseRedHologram = !isValidPlacement;

        // Choose per-option holograms if assigned, otherwise fall back to defaults
        GameObject optionHolo = (_currentBuildOption != null && _currentBuildOption.hologramPrefab != null)
            ? _currentBuildOption.hologramPrefab
            : hologramPrefab;

        GameObject optionRedHolo = (_currentBuildOption != null && _currentBuildOption.redHologramPrefab != null)
            ? _currentBuildOption.redHologramPrefab
            : redHologramPrefab;

        GameObject prefabToUse = shouldUseRedHologram ? optionRedHolo : optionHolo;

        // If prefab changes (different option) or color state changes, respawn hologram
        if (_currentHologram == null || _currentHologramIsRed != shouldUseRedHologram || _currentHologramPrefab != prefabToUse)
            SpawnHologram(prefabToUse, shouldUseRedHologram);

        if (_currentHologram == null)
            return; // Nothing to show (no prefabs assigned)

        _currentHologram.SetActive(true);
        _currentHologram.transform.position = hit.point;
        _currentHologram.transform.up = Vector3.up;
    }

    private void HideHologram()
    {
        if (_currentHologram != null)
            _currentHologram.SetActive(false);
    }


    private void HandlePlacement()
    {
        if (Mouse.current == null) return;
        if (_currentHologram == null || !_currentHologram.activeSelf || !_canPlaceOnCurrentTarget) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceObject();
        }
    }


    private void SpawnHologram(GameObject prefab, bool isRed)
    {
        if (_currentHologram != null)
            Destroy(_currentHologram);

        // Fallback to defaults if prefab is null
        if (prefab == null)
            prefab = isRed ? redHologramPrefab : hologramPrefab;

        if (prefab == null)
        {
            Debug.LogWarning("BuilderManager.SpawnHologram: no hologram prefab available to spawn.");
            _currentHologram = null;
            _currentHologramPrefab = null;
            return;
        }

        _currentHologram = Instantiate(prefab);
        _currentHologramIsRed = isRed;
        _currentHologramPrefab = prefab;
    }

    private void PlaceObject()
    {
        if (!_canPlaceOnCurrentTarget || _currentHologram == null || _currentBuildOption == null)
            return;

        if (_currentBuildOption.prefab == null)
        {
            Debug.LogWarning("BuilderManager.PlaceObject: selected build option has no prefab assigned.");
            return;
        }

        _isPlacing = false;
        Instantiate(
            _currentBuildOption.prefab,
            _currentHologram.transform.position,
            _currentHologram.transform.rotation
        );
    }

    private bool HasTagInParents(Transform target, string tagName)
    {
        while (target != null)
        {
            if (target.CompareTag(tagName))
                return true;

            target = target.parent;
        }

        return false;
    }
}
