using UnityEngine;

public class DrillAnimation : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float spinSpeed = 360f;

    private float cycleTime;
    private float startTime;
    void Start()
    {
        startTime = Time.time;
    }
    
    void Update()
    {
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);

        cycleTime = (Time.time - startTime) % 10f;

        if (cycleTime < 2f)
        {
            transform.position += Vector3.down * Time.deltaTime * moveSpeed;
        }

        else if (cycleTime > 6f && cycleTime < 8f)
        {
            transform.position += Vector3.up * Time.deltaTime * moveSpeed;
        }
    }
}