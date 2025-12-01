using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DroneHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text _irText;
    [SerializeField] private TMP_Text _stabilizationText;
    [SerializeField] private TMP_Text _lockerText;
    [SerializeField] private BoundsFloat _textColorMinMaxAlpha;
    [SerializeField] private TMP_Text _distanceText;
    [SerializeField] private TMP_Text _currentCompassAngleText;
    
    [Header("Drone Vizualizer Settings")]
    [SerializeField] private RectTransform _droneIcon;
    [SerializeField] private RectTransform _droneFieldOfView;
    [SerializeField] private float _droneRotationError;
    [SerializeField] private Slider _zoomSlider;
    [SerializeField] private RectTransform _reticleTransform;
    
    [Header("IDLE Shake")]
    [SerializeField] private float _shakeAmplitude = 0.25f;
    [SerializeField] private float _shakeFrequency = 2f;   
    
    [Header("Dynamic Shake")]
    [SerializeField] private float _moveShakeIntensity = 0.35f;
    [SerializeField] private float _rotateShakeIntensity = 0.25f;
    [SerializeField] private float _moveShakeFrequency = 1.4f;
    [SerializeField] private float _rotateShakeFrequency = 2.2f;

    private float _moveSeed;
    private float _rotateSeed;

    private ThermalVisionManager _thermalVisionManager;
    private DroneInput _droneInput;
    private DroneSensorSystem _droneSensorSystem;
    private DroneController _droneController;
    private DroneCompass _droneCompass;
    private Color IRTextColor => _irText.color;
    
    private Color LOCKTextColor => _stabilizationText.color;

    private Vector3 _basePosition;
    private Vector3 _iconBasePos;
    private Vector3 _fovBasePos;
    
    private float _shakeSeed;
    
    private Coroutine _irPulseRoutine;
    private Coroutine _lockerRoutine;
    

    public void Init(ThermalVisionManager thermalVisionManager, DroneInput droneInput,
        DroneSensorSystem droneSensorSystem, DroneCompass droneCompass, DroneController droneController)
    {
        _thermalVisionManager = thermalVisionManager;
        _droneInput = droneInput;
        _droneSensorSystem = droneSensorSystem;
        _droneCompass = droneCompass;
        _droneController = droneController;

        _droneCompass.OnYawChangedNormalized += UpdateCompassAngleText;
        _thermalVisionManager.OnThermalStateChanged += UpdateIRText;
        _droneSensorSystem.OnDistanceChange += UpdateDistanceText;
        _droneController.OnLockChanged += UpdateLockText;
        _droneController.OnYawChanged += UpdateDroneWorldRotation;
        _droneSensorSystem.OnYawChanged += UpdateDroneFieldOfViewWorldRotation;
        _droneSensorSystem.OnZoomChanged += UpdateZoomSlider;
        _droneSensorSystem.OnZoomChanged += UpdateReticleScale;

        _basePosition = transform.localPosition;
        _shakeSeed = Random.value * 100f;
        
        _moveSeed = Random.value * 50f;
        _rotateSeed = Random.value * 100f;
        
        _iconBasePos = _droneIcon.localPosition;
        _fovBasePos = _droneFieldOfView.localPosition;
        
        UpdateText(_irText, ref _irPulseRoutine, IRTextColor, _thermalVisionManager.IsActive);
        UpdateText(_lockerText, ref _lockerRoutine, LOCKTextColor, _droneController.IsLocked);
    }

    private void UpdateReticleScale(float value)
    {
        float scale = Mathf.Lerp(0.35f, 1f, value);
        _reticleTransform.localScale = new Vector3(scale, scale, 1);
    }

    private void UpdateZoomSlider(float value)
    {
        _zoomSlider.value = value;
    }

    private void OnDisable()
    {
        if (_thermalVisionManager != null)
            _thermalVisionManager.OnThermalStateChanged -= UpdateIRText;
        if (_droneSensorSystem != null)
            _droneSensorSystem.OnDistanceChange -= UpdateDistanceText;
        if(_droneCompass != null)
            _droneCompass.OnYawChangedNormalized -= UpdateCompassAngleText;
        _droneController.OnLockChanged -= UpdateLockText;
        _droneController.OnYawChanged -= UpdateDroneWorldRotation;
        _droneSensorSystem.OnYawChanged -= UpdateDroneFieldOfViewWorldRotation;
        _droneSensorSystem.OnZoomChanged -= UpdateZoomSlider;
        _droneSensorSystem.OnZoomChanged -= UpdateReticleScale;
    }

    private void Update()
    {
        ApplyShake();
        ApplyDynamicShake();
    }
    
    private void UpdateDroneWorldRotation(float yaw)
    {
        _droneIcon.localRotation = Quaternion.Euler(0, 0, -yaw + _droneRotationError);
    }
    private void UpdateDroneFieldOfViewWorldRotation(float yaw)
    {
        _droneFieldOfView.localRotation = Quaternion.Euler(0, 0, -yaw);
    }
    
    private void ApplyShake()
    {
        float time = Time.time * _shakeFrequency;
        float offsetX = (Mathf.PerlinNoise(_shakeSeed, time) - 0.5f) * _shakeAmplitude;
        float offsetY = (Mathf.PerlinNoise(_shakeSeed + 10f, time) - 0.5f) * _shakeAmplitude;

        transform.localPosition = _basePosition + new Vector3(offsetX, offsetY, 0f);
    }
    private void ApplyDynamicShake()
    {
        if (_droneController == null)
            return;

        float speed = _droneController.Speed;
        float rotSpeed = _droneController.AngularSpeed;

        if (float.IsNaN(speed) || float.IsInfinity(speed))
            speed = 0f;

        if (float.IsNaN(rotSpeed) || float.IsInfinity(rotSpeed))
            rotSpeed = 0f;

        float movePower = Mathf.Clamp01(speed / 25f);
        float rotPower = Mathf.Clamp01(rotSpeed / 90f);

        float t = Time.time;

        float moveX = (Mathf.PerlinNoise(_moveSeed, t * _moveShakeFrequency) - 0.5f)
                      * movePower * _moveShakeIntensity;

        float moveY = (Mathf.PerlinNoise(_moveSeed + 10f, t * _moveShakeFrequency) - 0.5f)
                      * movePower * _moveShakeIntensity;

        float rotX = (Mathf.PerlinNoise(_rotateSeed, t * _rotateShakeFrequency) - 0.5f)
                     * rotPower * _rotateShakeIntensity;

        float rotY = (Mathf.PerlinNoise(_rotateSeed + 20f, t * _rotateShakeFrequency) - 0.5f)
                     * rotPower * _rotateShakeIntensity;

        Vector3 totalOffset = new Vector3(moveX + rotX, moveY + rotY, 0f);

        _droneFieldOfView.localPosition = _fovBasePos + totalOffset;
    }

    private void UpdateIRText(bool active)
    {
        UpdateText(_irText, ref _irPulseRoutine, IRTextColor, active);
    }
    
    private void UpdateLockText(bool active)
    {
        UpdateText(_lockerText, ref _lockerRoutine, LOCKTextColor, active);
    }
    
    private void UpdateText(TMP_Text text, ref Coroutine coroutine, Color color, bool active)
    {
        float alpha = active ? _textColorMinMaxAlpha.Upper : _textColorMinMaxAlpha.Lower;
        text.color = new Color(color.r, color.g, color.b, alpha);

        if (active)
        {
            if (coroutine == null)
                coroutine = StartCoroutine(Pulse(text, alpha));
        }
        else
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        AudioManager.Instance.Play2D(SoundID.UI_HudSwitchClick, 0.3f);
    }

    private IEnumerator Pulse(TMP_Text text, float baseAlpha)
    {
        float time = 0f;
        const float speed = 2.5f;   
        const float range = 0.125f; 

        while (true)
        {
            time += Time.deltaTime * speed;
            float intensity = 1f + Mathf.Sin(time) * range;
            Color c = text.color;
            c.a = baseAlpha * intensity;
            text.color = c;
            yield return null;
        }
    }

    private void UpdateDistanceText(string distance)
    {
        _distanceText.text = $"DIST: {distance}";
    }

    private void UpdateCompassAngleText(float angle)
    {
        _currentCompassAngleText.text = $"{Mathf.RoundToInt(angle)}";
    }
}
