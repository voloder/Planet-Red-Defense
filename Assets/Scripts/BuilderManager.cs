using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuilderManager : MonoBehaviour
{
    public static BuilderManager Instance { get; private set; }

    [Header("Build Settings")]
    [SerializeField] private LayerMask buildSurfaceMask;
    [SerializeField] private float maxBuildDistance = 100f;
    [SerializeField] private AudioClip noResourcesClip;

    [System.Serializable]
    public class BuildOption
    {
        public string optionName;
        public Key key;
        public GameObject prefab;
        public bool isDrill;
        public GameObject hologramPrefab;
        public GameObject redHologramPrefab;

        // COST
        public int copperCost;
        public int ironCost;
        public int titaniumCost;
        public int diamondCost;
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
    
    public bool IsPlacing => _isPlacing;
    private bool _isPlacing;
    private bool _canPlaceOnCurrentTarget;
    private bool _currentHologramIsRed;
    private BuildOption _currentBuildOption;
    private string _currentOreTag;

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
                _currentOreTag = null;
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
            _currentOreTag = null;
            HideHologram();
            return;
        }
        if (Mouse.current == null || _cam == null)
        {
            _canPlaceOnCurrentTarget = false;
            _currentOreTag = null;
            HideHologram();
            return;
        }

        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance, buildSurfaceMask))
        {
            if (_currentBuildOption == null)
            {
                _canPlaceOnCurrentTarget = false;
                _currentOreTag = null;
                HideHologram();
                return;
            }

            string oreTag = GetOreTagInParents(hit.collider.transform);
            bool canPlace = _currentBuildOption.isDrill ? !string.IsNullOrEmpty(oreTag) : true;

            _currentOreTag = _currentBuildOption.isDrill ? oreTag : null;

            bool canAfford = ResourceManager.Instance == null || ResourceManager.Instance.CanUseResources(
                _currentBuildOption.copperCost,
                _currentBuildOption.ironCost,
                _currentBuildOption.titaniumCost,
                _currentBuildOption.diamondCost);

            _canPlaceOnCurrentTarget = canPlace;
            ShowHologram(hit, canPlace && canAfford);
        }
        else
        {
            _canPlaceOnCurrentTarget = false;
            _currentOreTag = null;
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

        if (!ResourceManager.Instance.UseResources(
            _currentBuildOption.copperCost,
            _currentBuildOption.ironCost,
            _currentBuildOption.titaniumCost,
            _currentBuildOption.diamondCost))
        {
            Debug.Log("Nema resursa!");
            if (noResourcesClip != null)
                AudioSource.PlayClipAtPoint(noResourcesClip, _cam.transform.position);
            return;
        }

        _isPlacing = false;
        GameObject placedObject = Instantiate(
            _currentBuildOption.prefab,
            _currentHologram.transform.position,
            _currentHologram.transform.rotation
        );

        if (_currentBuildOption.isDrill)
        {
            Drill drill = placedObject.GetComponent<Drill>();
            if (drill != null)
            {
                drill.ConfigureFromOreTag(_currentOreTag);
            }
            else
            {
                Debug.LogWarning("BuilderManager.PlaceObject: placed drill prefab has no Drill component.");
            }
        }

        _currentOreTag = null;
    }

    private string GetOreTagInParents(Transform target)
    {
        while (target != null)
        {
            if (target.CompareTag("CopperOre")) return "CopperOre";
            if (target.CompareTag("IronOre")) return "IronOre";
            if (target.CompareTag("TitaniumOre")) return "TitaniumOre";

            target = target.parent;
        }

        return null;
    }

}
