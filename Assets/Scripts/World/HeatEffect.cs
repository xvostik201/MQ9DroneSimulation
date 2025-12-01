using UnityEngine;

public class HeatEffect : MonoBehaviour
{
    private ParticleSystemRenderer _renderer;
    private void Awake()
    {
        _renderer = GetComponent<ParticleSystemRenderer>();
        ThermalVisionManager.RegisterHeatEffect(this);
    }

    private void OnDestroy()
    {
        ThermalVisionManager.UnregisterHeatEffect(this);
    }

    public void SetVisible(bool visible)
    {
        if (_renderer != null)
            _renderer.enabled = visible;
    }
}
