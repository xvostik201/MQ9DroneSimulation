using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Drone Config", fileName = "DroneConfig")]
public class DroneConfig : ScriptableObject
{
    [Header("Camera")]
    [Range(0.0001f, 0.00085f)] public float MinSensitivity = 0.3f;
    [Range(0.1f, 0.5f)] public float MaxSensitivity = 0.3f;
    [Range(0.1f, 100f)] public float ScrollSpeed = 5f;
    public BoundsFloat VerticalRotationBounds;
    [Range(5f, 1000f)] public float ZoomSmoothness = 1f;
    public BoundsFloat FOVBounds;
    public float MaxRangefinderDistance = 5500f;

    [Header("Thermal Vision")]
    public bool ThermalEnabledAtStart = false;
}