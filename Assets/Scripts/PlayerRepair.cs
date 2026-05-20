using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerRepair : MonoBehaviour
{
    public GameObject origin; // Where the shot starts (gun tip or player)
    public GameObject linePrefab; // The LineRenderer prefab
    public float maxDistance = 100f;
    public float lineDuration = 0.3f; // How long the line stays
    public GameObject target;

    public bool isShooting;

    void Update()
    {
        isShooting = false;
        // Fire when left mouse button is held
        if (Keyboard.current[Key.R].isPressed)
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

            Turret turret = originHit.collider.GetComponentInParent<Turret>();
            if (turret != null)
            {
                turret.Repair();
                Debug.Log("Repaired turret: " + turret.name);
            }
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

        Destroy(lineObj, lineDuration);
    }
}