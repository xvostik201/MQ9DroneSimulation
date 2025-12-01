using UnityEngine;

public class UIBootstrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DroneInput _droneInput;
    [SerializeField] private UIInput _uiInput;
    [SerializeField] private DroneSensorSystem _droneSensor;
    [SerializeField] private DroneController _droneController;
    [SerializeField] private DroneConfig _droneConfig;
    [SerializeField] private ThermalVisionManager _thermalVisionManager;
    [SerializeField] private CameraRaycaster _raycaster;
    [SerializeField] private DroneCompass _droneCompass;
    [SerializeField] private PaladinManager _paladinManager;
    
    [Header("UI Elements")]
    [SerializeField] private GameController _gameController;
    [SerializeField] private InputTips _inputTips;
    [SerializeField] private MapHUD _mapHUD;
    [SerializeField] private DroneHUD _droneHUD;
    [SerializeField] private HUDMessages _hudMessages;
    [SerializeField] private GameMenuUI _menuUI;
    [SerializeField] private SettingsUI _settingsUI;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _settingsUI.Init(_menuUI);
        _menuUI.Init(_uiInput, _droneInput,_settingsUI);
        _inputTips.Init(_gameController);
        _mapHUD.Init(_droneController, _droneSensor, _paladinManager);
        _droneHUD.Init(_thermalVisionManager, _droneInput, _droneSensor, _droneCompass, _droneController);
        
        UIService.RegisterHUD(_hudMessages);
    }
    
    private void OnDestroy()
    {
        UIService.UnregisterHUD();
    }
}