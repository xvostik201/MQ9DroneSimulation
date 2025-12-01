using UnityEngine;

public class DroneCameraLocker : MonoBehaviour
{
    private enum LockState
    {
        HardLook,
        SlerpLook
    }
    
    [Header("Settings")]
    [SerializeField] private float _rotationSpeed = 3f;
    [SerializeField] private LockState _lockState = LockState.HardLook;
    [SerializeField] private bool _debugLogs = true;
    [SerializeField] private bool _drawGizmos = true;

    private DroneSensorSystem _sensorSystem;
    private CameraRaycaster _raycaster;
    private DroneController _droneController;

    private bool _isLocked;
    private Vector3 _lockPosition;
    
    public bool IsLocked => _isLocked;

    public void Init(DroneSensorSystem sensorSystem, CameraRaycaster raycaster, DroneController droneController)
    {
        _sensorSystem = sensorSystem;
        _raycaster = raycaster;
        _droneController = droneController;

        _droneController.OnLockChanged += SwitchLock;
    }

    private void OnDisable()
    {
        if (_droneController != null)
            _droneController.OnLockChanged -= SwitchLock;
    }

    private void SwitchLock(bool locked)
    {
        _isLocked = locked;

        if (_isLocked)
        {
            Vector3 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
           
            if (_raycaster.TryGetHit(out RaycastHit hit, screenCenter))
            {
                _lockPosition = hit.point;

                if (_debugLogs)
                    Debug.Log($"[LOCKER] LOCKED on {hit.collider.name} at {_lockPosition}");
            }
            else if (_debugLogs)
                Debug.Log("[LOCKER] Lock pressed but raycast missed target.");
        }
        else
        {
            _sensorSystem.SyncManualRotationWithCurrent();
            Debug.Log("[LOCKER] UNLOCKED");
        }
    }

    public void UpdateLock()
    {
        if (!_isLocked || _lockPosition == Vector3.zero)
            return;

        Transform horiz = _sensorSystem.HorizontalTransform; 
        Transform vert  = _sensorSystem.VerticalTransform;   

        Vector3 worldDir = (_lockPosition - vert.position).normalized;

        Vector3 flatDir = new Vector3(worldDir.x, 0f, worldDir.z).normalized;
        float yaw = Quaternion.LookRotation(flatDir, Vector3.up).eulerAngles.y;

        Vector3 localDir = horiz.InverseTransformDirection(worldDir);
        float pitch = -Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

        Quaternion horizTarget = Quaternion.Euler(0f, yaw, 0f);
        Quaternion vertTarget  = Quaternion.Euler(pitch, 0f, 0f);

        switch (_lockState)
        {
            case LockState.HardLook:
                horiz.rotation = horizTarget;
                vert.localRotation = vertTarget;
                break;
            case LockState.SlerpLook:
                horiz.rotation = Quaternion.Slerp(horiz.rotation, horizTarget, Time.deltaTime * _rotationSpeed);
                vert.localRotation = Quaternion.Slerp(vert.localRotation, vertTarget, Time.deltaTime * _rotationSpeed);
                break;
        }
        
        

        if (_drawGizmos)
        {
            Debug.DrawLine(vert.position, _lockPosition, Color.red);
            Debug.DrawRay(vert.position, vert.forward * 5f, Color.cyan);
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 5f, Color.green);
        }

        if (_debugLogs)
        {
            Debug.Log(
                $"[LOCKER] Yaw:{yaw:F1}° Pitch:{pitch:F1}°\n" +
                $"Horiz rot:{horiz.rotation.eulerAngles}\n" +
                $"Vert rot:{vert.localRotation.eulerAngles}"
            );
        }
    }
}
