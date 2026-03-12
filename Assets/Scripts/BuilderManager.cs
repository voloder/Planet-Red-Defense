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

    [Header("Terrain")]
    [SerializeField] private Terrain terrain;

    [Header("Place key")]
    public Key placeKey = Key.Digit1;
    
    [Header("Cancel key")]
    public Key cancelKey = Key.Q;
    
    private bool isPlacing = false;

    private GameObject currentHologram;
    private Camera cam;

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
        cam = Camera.main;
    }

    private void Update()
    {
        if(Keyboard.current[placeKey].wasPressedThisFrame)
        {
            isPlacing = true;
        }
        
        if(Keyboard.current[cancelKey].wasPressedThisFrame)
        {
            isPlacing = false;
        }
        
        HandleRaycast();
        HandlePlacement();
    }

    private void HandleRaycast()
    {
        if(!isPlacing)
        {
            HideHologram();
            return;
        }
        if (Mouse.current == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, maxBuildDistance))
        {
            Terrain hitTerrain = hit.collider.GetComponent<Terrain>();
            if (hitTerrain != terrain)
            {
                HideHologram();
                return;
            }

            ShowHologram(hit);
        }
        else
        {
            HideHologram();
        }
    }


    private void ShowHologram(RaycastHit hit)
    {
        if (currentHologram == null)
            SpawnHologram();

        currentHologram.SetActive(true);
        currentHologram.transform.position = hit.point;
        currentHologram.transform.up = Vector3.up;
    }

    private void HideHologram()
    {
        if (currentHologram != null)
            currentHologram.SetActive(false);
    }


    private void HandlePlacement()
    {
        if (Mouse.current == null) return;
        if (currentHologram == null || !currentHologram.activeSelf) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceObject();
        }
    }


    private void SpawnHologram()
    {
        currentHologram = Instantiate(hologramPrefab);
    }

    private void PlaceObject()
    {
        isPlacing = false;
        Instantiate(
            buildablePrefab, // replace with real prefab
            currentHologram.transform.position,
            currentHologram.transform.rotation
        );
    }

    Vector3 Snap(Vector3 pos, float gridSize)
    {
        pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
        pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
        return pos;
    }
}
