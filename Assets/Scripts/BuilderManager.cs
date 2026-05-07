using UnityEngine;
using UnityEngine.InputSystem;

public class BuilderManager : MonoBehaviour
{
    public static BuilderManager Instance { get; private set; }

    [Header("Build Settings")]
    [SerializeField] private LayerMask buildSurfaceMask;
    [SerializeField] private float maxBuildDistance = 100f;

    [Header("Buildable Prefab")]
    [SerializeField] private GameObject buildablePrefab;

    [Header("Hologram")]
    [SerializeField] private GameObject hologramPrefab;
    [SerializeField] private GameObject redHologramPrefab;
    
    [Header("Terrain")]
    [SerializeField] private Terrain terrain;

    [Header("Place key")]
    public Key placeKey = Key.Digit1;
    
    [Header("Cancel key")]
    public Key cancelKey = Key.Q;
    
    private bool _isPlacing;
    private bool _canPlaceOnCurrentTarget;
    private bool _currentHologramIsRed;

    private GameObject _currentHologram;
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
            if(Keyboard.current[placeKey].wasPressedThisFrame)
            {
                _isPlacing = true;
            }
            
            if(Keyboard.current[cancelKey].wasPressedThisFrame)
            {
                _isPlacing = false;
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
            bool isCopperOre = HasTagInParents(hit.collider.transform, "CopperOre");
            _canPlaceOnCurrentTarget = isCopperOre;
            ShowHologram(hit, isCopperOre);
        }
        else
        {
            _canPlaceOnCurrentTarget = false;
            HideHologram();
        }
    }


    private void ShowHologram(RaycastHit hit, bool isCopperOre)
    {
        bool shouldUseRedHologram = !isCopperOre;
        if (_currentHologram == null || _currentHologramIsRed != shouldUseRedHologram)
            SpawnHologram(shouldUseRedHologram);

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


    private void SpawnHologram(bool useRedHologram)
    {
        if (_currentHologram != null)
            Destroy(_currentHologram);

        _currentHologram = Instantiate(useRedHologram ? redHologramPrefab : hologramPrefab);
        _currentHologramIsRed = useRedHologram;
    }

    private void PlaceObject()
    {
        if (!_canPlaceOnCurrentTarget || _currentHologram == null)
            return;

        _isPlacing = false;
        Instantiate(
            buildablePrefab, // replace with real prefab
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
