using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float lifetime = 1.5f;

    private float timer;
    private TextMeshPro textMesh;
    private Color originalColor;

    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        originalColor = textMesh.color;
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180f, 0);
        }

        timer += Time.deltaTime;
        float t = timer / lifetime;

        Color c = originalColor;
        c.a = Mathf.Lerp(1f, 0f, t);
        textMesh.color = c;

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}