using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerShoot : MonoBehaviour
{
    public GameObject origin; // Where the shot starts (gun tip or player)
    public GameObject linePrefab; // The LineRenderer prefab
    public float maxDistance = 100f;
    public float lineDuration = 0.3f; // How long the line stays
    public AudioClip shootClip; // Sound to play
    public AudioClip reloadClip;
    public GameObject target;

    [Header("Shooting Settings")] public float fireRate = 5f; // Shots per second
    public int magazineSize = 5; // Shots before reload
    public float reloadTime = 1.5f; // Seconds to reload


    private float nextFireTime = 0f;
    private int currentAmmo;
    private bool isReloading = false;

    public bool isShooting = false;

    void Start()
    {
        currentAmmo = magazineSize;
    }

    void Update()
    {
        isShooting = false;

        if (BuilderManager.Instance != null && BuilderManager.Instance.IsPlacing)
            return;

        if (Mouse.current.leftButton.isPressed)
        {
            isShooting = true;
            Ray cameraRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit cameraHit;

            Vector3 aimPoint;

            if (Physics.Raycast(cameraRay, out cameraHit, maxDistance))
                aimPoint = cameraHit.point;
            else return;

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
            aimPoint.y += 1f;
        }*/


        Vector3 shootDirection = (aimPoint - origin.transform.position).normalized;
        Ray originRay = new Ray(origin.transform.position, shootDirection);
        RaycastHit originHit;

        Vector3 finalHitPoint;

        if (Physics.Raycast(originRay, out originHit, maxDistance))
        {
            finalHitPoint = originHit.point;
            Debug.Log("Gun hit: " + originHit.collider.name);

            originHit.collider.GetComponent<Enemy>()?.TakeDamage(10f);
        }
        else
        {
            finalHitPoint = origin.transform.position + shootDirection * maxDistance;
        }

        GameObject lineObj = Instantiate(linePrefab);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, origin.transform.position);
        lr.SetPosition(1, finalHitPoint);


        Camera.main.GetComponent<AudioSource>().PlayOneShot(shootClip);

        Destroy(lineObj, lineDuration);
    }

    IEnumerator Reload()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(reloadClip);
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Reloaded!");
    }
}