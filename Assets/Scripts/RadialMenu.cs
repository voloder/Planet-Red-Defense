using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasRenderer))]
public class RadialMenuProcedural : MaskableGraphic
{
    [Header("Menu Settings")]
    public int optionCount = 3;
    public float innerRadius = 30f;
    public float outerRadius = 150f;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    [Header("Input")]
    public Key openKey = Key.Tab;

    private bool isOpen;
    private int highlightIndex = -1;
    private Vector2 centerScreen;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float step = 360f / optionCount;

        for (int i = 0; i < optionCount; i++)
        {
            float angleStart = i * step;
            float angleEnd = (i + 1) * step;

            Color sliceColor = (i == highlightIndex) ? highlightColor : normalColor;

            DrawSlice(vh, innerRadius, outerRadius, angleStart, angleEnd, sliceColor);
        }
    }

    private void DrawSlice(VertexHelper vh, float rInner, float rOuter, float angleStart, float angleEnd, Color color)
    {
        int steps = 20; // subdivide for smoothness
        float stepAngle = (angleEnd - angleStart) / steps;

        Vector2 prevInner = AngleToVector(angleStart) * rInner;
        Vector2 prevOuter = AngleToVector(angleStart) * rOuter;

        for (int i = 1; i <= steps; i++)
        {
            float angle = angleStart + i * stepAngle;
            Vector2 inner = AngleToVector(angle) * rInner;
            Vector2 outer = AngleToVector(angle) * rOuter;

            int startIndex = vh.currentVertCount;

            vh.AddVert(prevInner, color, Vector2.zero);
            vh.AddVert(prevOuter, color, Vector2.zero);
            vh.AddVert(outer, color, Vector2.zero);
            vh.AddVert(inner, color, Vector2.zero);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);

            prevInner = inner;
            prevOuter = outer;
        }
    }

    private Vector2 AngleToVector(float angle)
    {
        float rad = (angle + 90f) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
    private void Update()
    {
        // Open / close logic
        if (Keyboard.current[openKey].isPressed)
        {
            if (!isOpen)
                Open();
        }
        else if (isOpen)
        {
            Close();
        }

        // Update selection only when open
        if (isOpen)
            UpdateHighlight();
    }

    private void Open()
    {
        isOpen = true;
        gameObject.SetActive(true); // show UI
    }

    private void Close()
    {
        isOpen = false;
        gameObject.SetActive(false); // hide UI
        Debug.Log($"Selected option: {highlightIndex}");
    }

    private void UpdateHighlight()
    {
        Vector2 dir = Mouse.current.position.ReadValue() - centerScreen;

        if (dir.magnitude < innerRadius)
        {
            highlightIndex = -1;
        }
        else
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            angle = (angle + 270f) % 360f; // normalize 0–360
            angle = (angle) % 360f;   // rotate so slice 0 is at top
            highlightIndex = Mathf.FloorToInt(angle / (360f / optionCount)) % optionCount;

        }

        SetVerticesDirty(); // redraw mesh with new highlight
    }
}
