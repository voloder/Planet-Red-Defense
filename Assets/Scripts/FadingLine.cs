using System;
using UnityEngine;

public class FadingLine : MonoBehaviour
{
    private LineRenderer lr;
    public float duration = 0.1f; // How long the line lasts

    private float timer = 0f;
    private Gradient originalGradient;
    
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
    
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        originalGradient = lr.colorGradient; // Store original colors
    }

    private void Start()
    {
        SpawnOriginLight(lr.GetPosition(0));
        SpawnImpactLight(lr.GetPosition(1));
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