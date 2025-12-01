using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ThermalVisionManager : MonoBehaviour
{
    [SerializeField] private Volume _thermalVolume;
    [SerializeField] private bool _active = false;

    private static readonly List<HeatEffect> _heatEffects = new();

    public event Action<bool> OnThermalStateChanged;
    public bool IsActive => _active;

    private void Awake()
    {
        ApplyState();
    }

    public void Toggle()
    {
        SetActive(!_active);
    }

    public void SetActive(bool active)
    {
        if (_active == active)
            return;

        _active = active;
        ApplyState();
    }

    private void ApplyState()
    {
        Shader.SetGlobalFloat("_ThermalVisionEnabledGlobal", _active ? 1 : 0);
        _thermalVolume.enabled = _active;

        foreach (var fx in new List<HeatEffect>(_heatEffects))
            if (fx != null)
                fx.SetVisible(_active);

        OnThermalStateChanged?.Invoke(_active);
    }

    public static void RegisterHeatEffect(HeatEffect fx)
    {
        if (!_heatEffects.Contains(fx))
            _heatEffects.Add(fx);
    }

    public static void UnregisterHeatEffect(HeatEffect fx)
    {
        _heatEffects.Remove(fx);
    }
}