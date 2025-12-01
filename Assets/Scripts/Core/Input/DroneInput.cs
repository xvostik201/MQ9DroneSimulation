using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneInput : InputContextBase
{
    public event Action<Vector2> OnMouseDelta;
    public event Action<float> OnMouseScroll;
    public event Action OnThermalSwitch;
    public event Action OnCancel;
    public event Action OnOpenMap;
    public event Action OnMarkPress;
    public event Action OnCameraLockerPress;

    private Action<InputAction.CallbackContext> _mouseDelta;
    private Action<InputAction.CallbackContext> _scroll;
    private Action<InputAction.CallbackContext> _thermal;
    private Action<InputAction.CallbackContext> _openMap;
    private Action<InputAction.CallbackContext> _cancel;
    private Action<InputAction.CallbackContext> _onMarkPress;
    private Action<InputAction.CallbackContext> _onCameraLockerPress;

    protected override void EnableMap() => _input.Drone.Enable();
    protected override void DisableMap() => _input.Drone.Disable();

    protected override void Subscribe()
    {
        _mouseDelta = ctx => OnMouseDelta?.Invoke(ctx.ReadValue<Vector2>());
        _scroll     = ctx => OnMouseScroll?.Invoke(ctx.ReadValue<float>());
        _thermal    = _ => OnThermalSwitch?.Invoke();
        _openMap    = _ => OnOpenMap?.Invoke();
        _cancel   = _ => OnCancel?.Invoke();
        _onMarkPress = _ => OnMarkPress?.Invoke();
        _onCameraLockerPress = _ => OnCameraLockerPress?.Invoke();

        _input.Drone.MouseDelta.performed += _mouseDelta;
        _input.Drone.MouseScroll.performed += _scroll;
        _input.Drone.SwitchThermalVision.performed += _thermal;
        _input.Drone.Cancel.performed += _cancel;
        _input.Drone.OpenMap.performed += _openMap;
        _input.Drone.SetPoint.performed += _onMarkPress;
        _input.Drone.CameraLocker.performed += _onCameraLockerPress;
    }

    protected override void Unsubscribe()
    {
        _input.Drone.MouseDelta.performed -= _mouseDelta;
        _input.Drone.MouseScroll.performed -= _scroll;
        _input.Drone.SwitchThermalVision.performed -= _thermal;
        _input.Drone.OpenMap.performed -= _openMap;
        _input.Drone.SetPoint.performed -= _onMarkPress;
        _input.Drone.Cancel.performed += _cancel;
        _input.Drone.CameraLocker.performed -= _onCameraLockerPress;
    }
}