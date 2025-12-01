using System;
using UnityEngine;

public class DroneCompass : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform stripA;  
    [SerializeField] private RectTransform stripB;  

    [Header("Tuning")]
    [SerializeField] private float phasePx = 0f;     
    [SerializeField] private int dir = -1;         
    [SerializeField] private float overlapPx = 0.5f; 

    private float width;
    private float pxPerDeg;

    public event Action<float> OnYawChangedNormalized;

    public void Init(DroneSensorSystem sensor)
    {
        width    = stripA.rect.width;
        pxPerDeg = width / 360f;

        stripB.anchoredPosition = new Vector2(0f, 0f);
        stripA.anchoredPosition = new Vector2(-width + overlapPx, 0f);

        sensor.OnYawChanged += OnYawChanged;
    }

    private void OnYawChanged(float yawDeg)
    {
        float yaw = (yawDeg % 360f + 360f) % 360f;

        float raw = dir * yaw * pxPerDeg + phasePx;

        float bx = Mathf.Repeat(raw, width);

        float ax = bx - width + overlapPx;

        stripB.anchoredPosition = new Vector2(bx, 0f);
        stripA.anchoredPosition = new Vector2(ax, 0f);

        OnYawChangedNormalized?.Invoke(yaw);
    }
}