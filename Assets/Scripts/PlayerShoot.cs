using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerShoot : MonoBehaviour
{
    public GameObject origin;           // Where the shot starts (gun tip or player)
    public GameObject linePrefab;       // The LineRenderer prefab
    public float maxDistance = 100f;
    public float lineDuration = 0.3f;   // How long the line stays
    public AudioSource audioSource;     // AudioSource component
    public AudioClip shootClip;         // Sound to play
    public AudioClip reloadClip;
    public GameObject target;
    
    [Header("Shooting Settings")]
    public float fireRate = 5f;         // Shots per second
    public int magazineSize = 5;        // Shots before reload
    public float reloadTime = 1.5f;       // Seconds to reload

    
    [Header("Impact Light")]
    public float impactLightRange = 3f;
    public float impactLightIntensity = 4f;
    public float impactLightDuration = 0.1f;
    public Color impactLightColor = Color.white;
    
    [Header("Origin Light")]
    public float originLightRange = 3f;
    public float originLightIntensity = 4f;
    public float originLightDuration = 0.1f;
    public Color originLightColor = Color.white;
    
    private float nextFireTime = 0f;
    private int currentAmmo;
    private bool isReloading = false;

    public bool isShooting = false;
    
    void Start()
    {
        currentAmmo = magazineSize;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {   
        isShooting = false;
        // Fire when left mouse button is held
        if (Mouse.current.leftButton.isPressed )
        {        // 1. Ray from camera to mouse (aim point)
            isShooting = true;
            Ray cameraRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit cameraHit;

            Vector3 aimPoint;

            if (Physics.Raycast(cameraRay, out cameraHit, maxDistance))
            {
                aimPoint = cameraHit.point; // Slightly above hit point
            }
            else
            {
                return;
            }
            
            target.transform.position = aimPoint;
            
            if (currentAmmo > 0 && Time.time >= nextFireTime && !isReloading)
            {
                Shoot(aimPoint, cameraHit);
                currentAmmo--;
                nextFireTime = Time.time + 1f / fireRate;

                if (currentAmmo <= 0)
                {
                    StartCoroutine(Reload());
                }
            }
        }
    }

    void Shoot(Vector3 aimPoint, RaycastHit cameraHit)
    {


        /*if (cameraHit.collider.gameObject.tag == "Terrain")
        {
            aimPoint.y += 1.5f;
        }*/

        
        Vector3 shootDirection = (aimPoint - origin.transform.position).normalized;
        Ray originRay = new Ray(origin.transform.position, shootDirection);
        RaycastHit originHit;

        Vector3 finalHitPoint;

        if (Physics.Raycast(originRay, out originHit, maxDistance))
        {
            finalHitPoint = originHit.point;
            Debug.Log("Gun hit: " + originHit.collider.name);
        }
        else
        {
            finalHitPoint = origin.transform.position + shootDirection * maxDistance;
        }

        // 3. Spawn line prefab
        GameObject lineObj = Instantiate(linePrefab);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, origin.transform.position);
        lr.SetPosition(1, finalHitPoint);

        // 4. Sound
        if (audioSource && shootClip)
        {
            audioSource.PlayOneShot(shootClip);
        }
        SpawnImpactLight(finalHitPoint);
        SpawnOriginLight(origin.transform.position);
        Destroy(lineObj, lineDuration);
    }

    IEnumerator Reload()
    {
        if (audioSource && reloadClip)
        {
            audioSource.PlayOneShot(reloadClip);
        }
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Reloaded!");
    }
    
    void SpawnImpactLight(Vector3 position)
    {
        GameObject lightObj = new GameObject("ImpactLight");
        lightObj.transform.position = position;

        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = impactLightRange;
        light.intensity = impactLightIntensity;
        light.color = impactLightColor;
        light.shadows = LightShadows.None;

        Destroy(lightObj, impactLightDuration);
    }
    void SpawnOriginLight(Vector3 position)
    {
        GameObject lightObj = new GameObject("OriginLight");
        lightObj.transform.position = position;

        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = originLightRange;
        light.intensity = originLightIntensity;
        light.color = originLightColor;
        light.shadows = LightShadows.None;

        Destroy(lightObj, impactLightDuration);
    }

}
