using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMine : MonoBehaviour
{
    public GameObject origin; // Where the shot starts (gun tip or player)
    public GameObject linePrefab; // The LineRenderer prefab
    public float maxDistance = 100f;
    public float lineDuration = 0.3f; // How long the line stays
    public GameObject target;


    [Header("Impact Light")] public float impactLightRange = 3f;
    public float impactLightIntensity = 4f;
    public float impactLightDuration = 0.1f;
    public Color impactLightColor = Color.white;

    public float lastMineTime;

    public bool isShooting;

    void Update()
    {
        isShooting = false;
        // Fire when left mouse button is held
        if (Keyboard.current[Key.F].isPressed)
        {
            isShooting = true;
            Ray cameraRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit cameraHit;

            Vector3 aimPoint;

            if (Physics.Raycast(cameraRay, out cameraHit, maxDistance))
            {
                aimPoint = cameraHit.point;
            }
            else return;


            target.transform.position = aimPoint;

            if (Time.time - lastMineTime > 1f)
            {
                ResourceManager.Instance.AddResourcesWithFloatingText(1, 0, 0, 0, aimPoint);
                lastMineTime = Time.time;
            }

            Shoot(aimPoint);
        }
    }

    void Shoot(Vector3 aimPoint)
    {
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

        SpawnImpactLight(finalHitPoint);
        Destroy(lineObj, lineDuration);
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
}