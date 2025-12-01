using UnityEngine;
using UnityEngine.Rendering;

public class LightingBootstrap : MonoBehaviour
{
    [SerializeField] private Light _mainLight;

    [SerializeField] private AmbientMode _ambientMode = AmbientMode.Skybox;

    [SerializeField] private Color _ambientColor = Color.gray;

    private void Awake()
    {
        ApplyLightingSettings();
    }

    private void ApplyLightingSettings()
    {
        if (_mainLight == null)
            _mainLight = FindObjectOfType<Light>();

        if (_mainLight != null)
            RenderSettings.sun = _mainLight;

        RenderSettings.ambientMode = _ambientMode;
        RenderSettings.ambientLight = _ambientColor;

        DynamicGI.UpdateEnvironment(); 
    }
}