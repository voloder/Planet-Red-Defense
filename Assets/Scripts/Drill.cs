using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Every10Seconds : MonoBehaviour
{
    float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 10f)
        {
            timer = 0f;
            MineResource();
        }
    }

    void MineResource()
    {
        ResourceManager.Instance.AddResourcesWithFloatingText(10, 0, 0, 0, 
            transform.position + Vector3.up * 4);
    }
}