using UnityEngine;

public class CameraDebugRay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform _lockTarget;
    [SerializeField] private Color _forwardColor = Color.cyan;
    [SerializeField] private Color _toTargetColor = Color.red;
    [SerializeField] private float _rayLength = 5000f;
    [SerializeField] private float _markerSize = 0.5f;
    [SerializeField] private bool _showAngle = true;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
            _camera = Camera.main;
    }

    private void Update()
    {
        if (_camera == null) return;

        Vector3 origin = _camera.transform.position;
        Vector3 forward = _camera.transform.forward;

        Debug.DrawRay(origin, forward * _rayLength, _forwardColor);

        if (_lockTarget != null)
        {
            Vector3 toTarget = (_lockTarget.position - origin).normalized;
            Debug.DrawRay(origin, toTarget * _rayLength, _toTargetColor);

            if (_showAngle)
            {
                float angle = Vector3.Angle(forward, toTarget);
                Debug.Log($"Camera error angle: {angle:F3}°");
            }

            Debug.DrawLine(_lockTarget.position + Vector3.up * _markerSize, _lockTarget.position - Vector3.up * _markerSize, _toTargetColor);
            Debug.DrawLine(_lockTarget.position + Vector3.right * _markerSize, _lockTarget.position - Vector3.right * _markerSize, _toTargetColor);
        }
    }

    public void SetLockTarget(Vector3 worldPos)
    {
        if (_lockTarget == null)
        {
            GameObject marker = new GameObject("DebugLockMarker");
            _lockTarget = marker.transform;
        }
        _lockTarget.position = worldPos;
    }
}
