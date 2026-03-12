using UnityEngine;

public class FadingLine : MonoBehaviour
{
    private LineRenderer lr;
    public float duration = 0.1f; // How long the line lasts

    private float timer = 0f;
    private Gradient originalGradient;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        originalGradient = lr.colorGradient; // Store original colors
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        // Fade color alpha
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = originalGradient.colorKeys;
        GradientAlphaKey[] alphaKeys = originalGradient.alphaKeys;

        // Lerp alpha
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            alphaKeys[i].alpha = Mathf.Lerp(alphaKeys[i].alpha, 0f, t);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        lr.colorGradient = gradient;

        // Shrink line width over time
        float startWidth = Mathf.Lerp(lr.startWidth, 0f, t);
        float endWidth = Mathf.Lerp(lr.endWidth, 0f, t);
        lr.startWidth = startWidth;
        lr.endWidth = endWidth;

        // Destroy when fully faded
        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}