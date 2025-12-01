using UnityEngine;

public class DroneBootstrap : MonoBehaviour
{
    [SerializeField] private DroneInput _input;
    [SerializeField] private DroneSensorSystem _droneSensor;
    [SerializeField] private DroneController _droneController;
    [SerializeField] private DroneConfig _droneConfig;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private ThermalVisionManager _thermalVisionManager;
    [SerializeField] private DroneHUD _droneHUD;
    [SerializeField] private CameraRaycaster _raycaster;
    [SerializeField] private DroneCompass _droneCompass;
    [SerializeField] private MapHUD _mapHUD;
    [SerializeField] private DroneCameraLocker _cameraLocker;

    private void Awake()
    {
        CursorManager.Lock();

        _droneSensor.Init(_droneConfig, _input, _mainCamera, _raycaster, _droneController);
        _droneController.Init(_input, _thermalVisionManager, _raycaster);
        _cameraLocker.Init(_droneSensor, _raycaster, _droneController);
        _droneSensor.SetLocker(_cameraLocker);

        _raycaster.Init(_mainCamera, _droneConfig);
        _droneCompass.Init(_droneSensor);
    }
}