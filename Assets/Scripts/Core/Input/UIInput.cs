using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInput : InputContextBase
{
    public event Action<Vector2> OnNavigate;
    public event Action OnSubmit;
    public event Action OnCancel;
    public event Action OnClick;
    public event Action OnOpenMap;
    public event Action OnSetMark;

    private Action<InputAction.CallbackContext> _navigate;
    private Action<InputAction.CallbackContext> _submit;
    private Action<InputAction.CallbackContext> _cancel;
    private Action<InputAction.CallbackContext> _click;
    private Action<InputAction.CallbackContext> _openMap;
    private Action<InputAction.CallbackContext> _setMark;

    protected override void EnableMap() => _input.UI.Enable();
    protected override void DisableMap() => _input.UI.Disable();

    protected override void Subscribe()
    {
        _navigate = ctx => OnNavigate?.Invoke(ctx.ReadValue<Vector2>());
        _submit   = _ => OnSubmit?.Invoke();
        _cancel   = _ => OnCancel?.Invoke();
        _click    = _ => OnClick?.Invoke();
        _openMap  = _ => OnOpenMap?.Invoke();
        _setMark  = _ => OnSetMark?.Invoke();

        _input.UI.Navigate.performed += _navigate;
        _input.UI.Submit.performed += _submit;
        _input.UI.Cancel.performed += _cancel;
        _input.UI.Click.performed += _click;
        _input.UI.OpenMap.performed += _openMap;
        _input.UI.SetMark.performed += _setMark;
    }

    protected override void Unsubscribe()
    {
        _input.UI.Navigate.performed -= _navigate;
        _input.UI.Submit.performed -= _submit;
        _input.UI.Cancel.performed -= _cancel;
        _input.UI.Click.performed -= _click;
        _input.UI.OpenMap.performed -= _openMap;
        _input.UI.SetMark.performed -= _setMark;
    }
}