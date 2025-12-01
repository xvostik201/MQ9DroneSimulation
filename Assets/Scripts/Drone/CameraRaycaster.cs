using System;
using UnityEngine;

public class CameraRaycaster : MonoBehaviour
{
    private Camera _camera;
    private LayerMask _mask = Physics.DefaultRaycastLayers;
    private DroneConfig _droneConfig;

    public void Init(Camera cam, DroneConfig config)
    {
        _camera = cam;
        _droneConfig = config;
    }

    public bool TryGetHit(out RaycastHit hit)
    {
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        return Physics.Raycast(ray, out hit, _droneConfig.MaxRangefinderDistance);
    }

    public bool TryGetHit(out RaycastHit hit, Vector3 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out hit, _droneConfig.MaxRangefinderDistance);
    }

    public string GetFormattedDistance()
    {
        if (TryGetHit(out RaycastHit hit))
        {
            float distance = Mathf.RoundToInt(hit.distance);
            return hit.distance switch
            {
                > 1000 => $"{distance}",
                > 100  => $"-{distance}",
                < 100  => $"--{distance}",
                _      => $"{distance}"
            };
        }
        return "----";
    }
}