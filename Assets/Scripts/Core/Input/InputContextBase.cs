using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InputContextBase : MonoBehaviour
{
    protected MainMap _input;

    protected virtual void Awake()
    {
        _input = new MainMap();
    }

    protected virtual void OnEnable()
    {
        EnableMap();
        Subscribe();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
        DisableMap();
    }

    protected abstract void EnableMap();
    protected abstract void DisableMap();
    protected abstract void Subscribe();
    protected abstract void Unsubscribe();
}