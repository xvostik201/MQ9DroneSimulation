using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelUI _levelUI;
    [SerializeField] private PaladinManager _paladinManager;
    [SerializeField] private MissionManager _missionManager;
    
    [SerializeField] private HUDMessages _hudMessages;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private DroneInput _droneInput;
    [SerializeField] private UIInput _uiInput;
    [SerializeField] private MapHUD _mapHUD;
    [SerializeField] private DroneController _drone;
    [SerializeField] private CameraRaycaster _cameraRaycaster;
    [SerializeField] private GameObject _mapObject;

    private bool _mapActive;

    public event Action<bool> OnMapActive;
    public event Action<Vector3> OnMarkPlaced;

    private void OnEnable()
    {
        _droneInput.OnOpenMap += ToggleMap;
        _uiInput.OnOpenMap += ToggleMap;

        _uiInput.OnSetMark += HandleSetMark;
        _droneInput.OnMarkPress += HandleSetMark;

        _uiInput.OnSubmit += HandleFireCommand;

        _levelUI.OnContinueButtonPressed += RestartMission;
        
        OnMapActive?.Invoke(_mapActive);
        _mapObject.SetActive(_mapActive);
    }

    private void OnDisable()
    {
        _droneInput.OnOpenMap -= ToggleMap;
        _uiInput.OnOpenMap -= ToggleMap;

        _uiInput.OnSetMark -= HandleSetMark;
        _droneInput.OnMarkPress -= HandleSetMark;
        
        _uiInput.OnSubmit -= HandleFireCommand;
        
        _levelUI.OnContinueButtonPressed -= RestartMission;
    }

    private void ToggleMap()
    {
        _mapActive = !_mapActive;
        _mapObject.SetActive(_mapActive);

        if (_mapActive)
        {
            CursorManager.Unlock();
            _inputManager.SwitchToUI();
        }
        else
        {
            CursorManager.Lock();
            _inputManager.SwitchToDrone();
        }
        
        AudioManager.Instance.PlayUI(SoundID.UI_Click, AudioManager.Instance.SfxVolume);

        OnMapActive?.Invoke(_mapActive);
    }
    public void RestartMission()
    {
        _mapHUD.ResetMap();

        _paladinManager.DestroyAllPaladins();

        _missionManager.ResetMission();

        _missionManager.LaunchNextMission();
    }


    private void HandleFireCommand()
    {
        if (!_mapActive)
            return;

        _drone.ConfirmFire();
        UIService.HUD.ShowMessage("FIRE FIRE FIRE");
    }
    
    private void HandleSetMark()
    {
        Vector3 worldPoint;

        if (_mapActive)
        {
            if (_mapHUD.TryGetMapClickPosition(out worldPoint))
            {
                PlaceMark(worldPoint);
            }
        }
        else
        {
            Vector3 mousePos = Input.mousePosition;
            if (_cameraRaycaster.TryGetHit(out RaycastHit hit, mousePos))
            {
                worldPoint = hit.point;
                PlaceMark(worldPoint);
            }
        }
    }

    private void PlaceMark(Vector3 worldPoint)
    {
        _mapHUD.UpdateMarkPosition(worldPoint);
        _hudMessages?.ShowMessage("MARK SETTED");
        OnMarkPlaced?.Invoke(worldPoint);
    }
}
