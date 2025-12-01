using System;
using UnityEngine;

public class DroneSensorSystem : MonoBehaviour
{
    [Header("Rotation Mechanisms")]
    [SerializeField] private Transform _horizontalRotationMechanism;
    [SerializeField] private Transform _verticalRotationMechanism;

    [Header("Audio")]
    [SerializeField] private AudioSource _gimbalRotateSource;

    [Header("Gimbal Audio Settings")]
    [SerializeField] private float minDeltaToPlay = 0.01f;
    [SerializeField] private float volumeScale = 0.001f;
    [SerializeField] private float maxVolume = 0.125f;
    [SerializeField] private float pitchBase = 1f;
    [SerializeField] private float pitchMax = 1.1f;
    [SerializeField] private float deltaPitchScale = 0.015f;
    [SerializeField] private float fadeSpeed = 10f;
    [SerializeField] private float inertiaFade = 10f;

    private DroneInput _input;
    private Camera _droneCamera;
    private DroneConfig _droneConfig;
    private CameraRaycaster _raycaster;
    private DroneController _controller;
    private DroneCameraLocker _locker;

    private Vector2 _rotation;
    private float _currentFov;
    private float _zoomPercent;

    private float _gimbalDelta;
    private float _gimbalTargetVolume;
    private float _gimbalTargetPitch;

    public event Action<float> OnZoomChanged;
    public event Action<string> OnDistanceChange;
    public event Action<float> OnYawChanged;

    public Vector2 CurrentRotation => _rotation;
    public Transform HorizontalTransform => _horizontalRotationMechanism;
    public Transform VerticalTransform => _verticalRotationMechanism;

    public void Init(DroneConfig droneConfig, DroneInput input, Camera cam, CameraRaycaster raycaster, DroneController controller)
    {
        _droneConfig = droneConfig;
        _input = input;
        _droneCamera = cam;
        _raycaster = raycaster;
        _controller = controller;

        _input.OnMouseScroll += HandleMouseScroll;
        _input.OnMouseDelta += HandleMouseDelta;
    }

    private void Start()
    {
        _currentFov = _droneCamera.fieldOfView;

        _gimbalRotateSource.clip = AudioManager.Instance.Get(SoundID.GimbalRotate);
        AudioManager.Instance.RegisterSource(_gimbalRotateSource);

        float percent = Mathf.InverseLerp(
            _droneConfig.FOVBounds.Lower,
            _droneConfig.FOVBounds.Upper,
            _droneCamera.fieldOfView
        );

        OnZoomChanged?.Invoke(percent);
    }

    private void OnDisable()
    {
        if (_input != null)
        {
            _input.OnMouseScroll -= HandleMouseScroll;
            _input.OnMouseDelta -= HandleMouseDelta;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.UnregisterSource(_gimbalRotateSource);
    }

    private void Update()
    {
        UpdateGimbalAudio();
    }

    private void HandleMouseDelta(Vector2 delta)
    {
        if (TimeManager.Instance.IsPaused || TimeManager.Instance.IsGameplayFrozen) return;
        if (_controller.IsLocked) return;

        float magnitude = delta.magnitude;
        _gimbalDelta = magnitude;

        if (magnitude > minDeltaToPlay)
        {
            _gimbalTargetVolume = Mathf.Clamp(magnitude * volumeScale, 0f, maxVolume);
            _gimbalTargetPitch = Mathf.Lerp(pitchBase, pitchMax, magnitude * deltaPitchScale);
        }
        else
        {
            _gimbalTargetVolume = 0f;
        }

        float sensitivityPercent = Mathf.InverseLerp(
            _droneConfig.FOVBounds.Lower,
            _droneConfig.FOVBounds.Upper,
            _droneCamera.fieldOfView
        );

        float scaledSensitivity = Mathf.Lerp(
            SettingsManager.Data.SensMin,
            SettingsManager.Data.SensMax,
            sensitivityPercent
        );

        _rotation.x += delta.x * scaledSensitivity;
        _rotation.y -= delta.y * scaledSensitivity;

        _rotation.y = Mathf.Clamp(
            _rotation.y,
            _droneConfig.VerticalRotationBounds.Lower,
            _droneConfig.VerticalRotationBounds.Upper
        );

        _horizontalRotationMechanism.localRotation = Quaternion.Euler(0f, _rotation.x, 0f);
        _verticalRotationMechanism.localRotation = Quaternion.Euler(_rotation.y, 0f, 0f);

        OnDistanceChange?.Invoke(_raycaster.GetFormattedDistance());
        OnYawChanged?.Invoke(_rotation.x);
    }

    private void HandleMouseScroll(float scroll)
    {
        if (TimeManager.Instance.IsPaused) return;

        _zoomPercent += scroll * SettingsManager.Data.ScrollSpeed;
        _zoomPercent = Mathf.Clamp01(_zoomPercent);

        ApplyZoom();
    }

    private void ApplyZoom()
    {
        float targetFov = Mathf.Lerp(
            _droneConfig.FOVBounds.Upper,
            _droneConfig.FOVBounds.Lower,
            _zoomPercent
        );

        _currentFov = targetFov;

        _droneCamera.fieldOfView = Mathf.MoveTowards(
            _droneCamera.fieldOfView,
            _currentFov,
            Time.deltaTime * _droneConfig.ZoomSmoothness
        );

        float percent = Mathf.InverseLerp(
            _droneConfig.FOVBounds.Lower,
            _droneConfig.FOVBounds.Upper,
            _droneCamera.fieldOfView
        );

        OnZoomChanged?.Invoke(percent);
    }

    private void LateUpdate()
    {
        if (_locker == null) return;
        if (!_locker.IsLocked) return;

        _locker.UpdateLock();

        Vector3 horizAngles = _horizontalRotationMechanism.localEulerAngles;
        Vector3 vertAngles = _verticalRotationMechanism.localEulerAngles;

        _rotation.x = horizAngles.y;
        _rotation.y = vertAngles.x;

        OnYawChanged?.Invoke(_rotation.x);
    }

    private void UpdateGimbalAudio()
    {
        _gimbalDelta = Mathf.Lerp(_gimbalDelta, 0f, Time.deltaTime * inertiaFade);

        if (_gimbalTargetVolume > 0.001f && _gimbalDelta > minDeltaToPlay)
        {
            if (TimeManager.Instance.IsPaused || TimeManager.Instance.IsGameplayFrozen)
            {
                if (_gimbalRotateSource.isPlaying)
                    _gimbalRotateSource.Pause();

                return;
            }

            if (!_gimbalRotateSource.isPlaying)
                _gimbalRotateSource.Play();

            float sfx = AudioManager.Instance.SfxVolume;

            float targetVol = _gimbalTargetVolume * sfx;

            _gimbalRotateSource.volume = Mathf.Lerp(
                _gimbalRotateSource.volume,
                targetVol,
                Time.deltaTime * fadeSpeed
            );

            _gimbalRotateSource.pitch = Mathf.Lerp(
                _gimbalRotateSource.pitch,
                _gimbalTargetPitch,
                Time.deltaTime * fadeSpeed
            );
        }
        else
        {
            if (_gimbalRotateSource.isPlaying)
            {
                _gimbalRotateSource.volume = Mathf.Lerp(
                    _gimbalRotateSource.volume,
                    0f,
                    Time.deltaTime * fadeSpeed
                );

                if (_gimbalRotateSource.volume < 0.01f)
                    _gimbalRotateSource.Stop();
            }
        }
    }

    public void SyncManualRotationWithCurrent()
    {
        Vector3 horizAngles = _horizontalRotationMechanism.localEulerAngles;
        Vector3 vertAngles = _verticalRotationMechanism.localEulerAngles;

        _rotation.x = horizAngles.y;
        _rotation.y = vertAngles.x;
    }

    public void SetLocker(DroneCameraLocker locker)
    {
        _locker = locker;
    }
}
