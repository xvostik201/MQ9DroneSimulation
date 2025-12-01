using System;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private DronePathBase path;
    [SerializeField] private float speed = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource _engineSource;
    [SerializeField] private AudioSource _engineWhineSource;

    [Header("Propeller")]
    [SerializeField] private Transform _propeller;
    [SerializeField] private float _rotateSpeed = 900f;

    private DroneInput _droneInput;
    private ThermalVisionManager _thermalVisionManager;
    private CameraRaycaster _cameraRaycaster;

    public event Action<Vector3> OnPositionChanged;
    public event Action<float> OnYawChanged;
    public event Action<DronePathBase> OnPathChanged;
    public event Action<Vector3> OnMarkPlaced;
    public event Action<bool> OnLockChanged;
    public event Action OnFireConfirmed;

    public bool IsLocked { get; private set; }
    public float Speed { get; private set; }
    public float AngularSpeed { get; private set; }
    public DronePathBase CurrentPath => path;

    private float _t;
    private Vector3 _prevPos;
    private float _prevYaw;

    private Vector3 _futurePos;
    private float _futureT;

    public void Init(DroneInput input, ThermalVisionManager thermalVisionManager, CameraRaycaster raycaster)
    {
        _droneInput = input;
        _cameraRaycaster = raycaster;
        _thermalVisionManager = thermalVisionManager;

        _droneInput.OnThermalSwitch += _thermalVisionManager.Toggle;
        _droneInput.OnCameraLockerPress += SwitchLock;
        _droneInput.OnMarkPress += SetMark;
    }

    private void Start()
    {
        path?.Init(transform);

        _engineSource.clip = AudioManager.Instance.Get(SoundID.DroneEngine);
        _engineWhineSource.clip = AudioManager.Instance.Get(SoundID.DroneWhine);

        AudioManager.Instance.RegisterSource(_engineSource);
        AudioManager.Instance.RegisterSource(_engineWhineSource);

        _engineSource.Play();
        _engineWhineSource.Play();

        OnPositionChanged?.Invoke(transform.position);
    }

    private void Update()
    {
        UpdatePath();
        UpdateRotation();
        UpdateSpeedData();
        UpdateAudio();
        UpdatePropeller();
        DispatchEvents();
    }

    private void UpdatePath()
    {
        if (path == null) return;

        _t += Time.deltaTime * speed;
        if (_t > path.Duration) _t -= path.Duration;

        path.Evaluate(_t, out var pos, out _);
        transform.position = pos;

        _futureT = _t + 0.05f;
        if (_futureT > path.Duration) _futureT -= path.Duration;

        path.Evaluate(_futureT, out _futurePos, out _);
    }

    private void UpdateRotation()
    {
        Vector3 moveDir = (_futurePos - transform.position).normalized;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
        }
    }

    private void UpdateSpeedData()
    {
        Vector3 delta = transform.position - _prevPos;
        Speed = delta.magnitude / Time.deltaTime;
        _prevPos = transform.position;

        float yaw = transform.rotation.eulerAngles.y;
        float yawDelta = Mathf.DeltaAngle(_prevYaw, yaw);
        AngularSpeed = Mathf.Abs(yawDelta) / Time.deltaTime;
        _prevYaw = yaw;
    }

    private void UpdateAudio()
    {
        float t = Mathf.InverseLerp(0f, 60f, Speed);
        float sfx = AudioManager.Instance.SfxVolume;

        _engineSource.pitch = Mathf.Lerp(0.85f, 1.25f, t);
        _engineSource.volume = Mathf.Lerp(0.40f, 0.60f, t) * sfx;

        _engineWhineSource.pitch = Mathf.Lerp(1.1f, 1.8f, t);
        _engineWhineSource.volume = Mathf.Lerp(0.10f, 0.30f, t) * sfx;
    }

    private void UpdatePropeller()
    {
        _propeller.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime);
    }

    private void DispatchEvents()
    {
        OnPositionChanged?.Invoke(transform.position);
        OnYawChanged?.Invoke(transform.rotation.eulerAngles.y);
    }

    private void SwitchLock()
    {
        IsLocked = !IsLocked;
        OnLockChanged?.Invoke(IsLocked);
    }

    public void ConfirmFire()
    {
        OnFireConfirmed?.Invoke();
    }

    private void SetMark()
    {
        Vector3 mousePos = Input.mousePosition;

        if (_cameraRaycaster.TryGetHit(out RaycastHit hit, mousePos))
        {
            Vector3 worldPoint = hit.point;
            OnMarkPlaced?.Invoke(worldPoint);
        }
    }

    public void SetPath(DronePathBase newPath, bool keepProgress = false)
    {
        path = newPath;
        path?.Init(transform);

        if (!keepProgress)
            _t = 0f;

        OnPathChanged?.Invoke(path);
    }

    private void OnDisable()
    {
        if (_droneInput != null)
        {
            _droneInput.OnThermalSwitch -= _thermalVisionManager.Toggle;
            _droneInput.OnCameraLockerPress -= SwitchLock;
            _droneInput.OnMarkPress -= SetMark;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UnregisterSource(_engineSource);
            AudioManager.Instance.UnregisterSource(_engineWhineSource);
        }
    }
}
