using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Paladin : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _tower;
    [SerializeField] private Transform _gun;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Transform _cannonPivot;

    [Header("Shooting")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _shootForce = 150f;
    [SerializeField] private float _shootRate = 3f;
    [SerializeField] private int _maxAmmo = 10;
    [SerializeField] private float _reloadTime = 2f;

    [Header("Spread & Recoil")]
    [SerializeField] private bool _useSpread = true;
    [SerializeField] private float _minSpreadAngle = 0f;
    [SerializeField] private float _maxSpreadAngle = 2f;
    [SerializeField] private float _gunRecoilDistance = 0.2f;
    [SerializeField] private float _gunRecoilSpeed = 4f;

    [Header("Aiming")]
    [SerializeField] private float _towerRotationSpeed = 12f;
    [SerializeField] private float _gunRotationSpeed = 7f;

    [SerializeField] private float _aimThreshold = 1f;

    [Header("Trajectory")]
    [SerializeField] private float _timeStep = 0.05f;
    [SerializeField] private int _maxSteps = 500;
    [SerializeField] private float _minElevationAngle = -4f;
    [SerializeField] private float _maxElevationAngle = 85f;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private bool _drawTrajectories = false;
    [SerializeField] private float _drawInterval = 0.1f;

    [Header("FX")]
    [SerializeField] private ParticleSystem _muzzleFire;
    [SerializeField] private ParticleSystem _muzzleSmoke;

    private int _currentAmmo;
    private float _reloadTimer;
    private float _shootTimer;
    private Quaternion _desiredTowerRotation;
    private Quaternion _desiredGunRotation;
    private bool _isAiming;
    private Vector3 _gunInitialLocalPos;
    private Terrain _terrain;
    private float _terrainBaseY;
    private Vector3 _target;
    
    public Vector3 TargetPosition => _target;

    public Vector3[] LastTrajectory { get; private set; }
    public float CurrentTimeToHit { get; private set; }

    public event Action OnAimed;
    public event Action OnFired;

    public PaladinStatus Status { get; private set; } = PaladinStatus.Idle;
    public event Action<Paladin, PaladinStatus> OnStatusChanged;

    private void SetStatus(PaladinStatus newStatus)
    {
        if (Status == newStatus) return;
        Status = newStatus;
        OnStatusChanged?.Invoke(this, newStatus);
        Debug.Log($"[Paladin] {name} → {newStatus}");
    }

    private void Awake()
    {
        FindObjectOfType<PaladinManager>()?.RegisterPaladin(this);
        
        _gunInitialLocalPos = _cannonPivot.localPosition;
        _terrain = Terrain.activeTerrain;
        _terrainBaseY = _terrain ? _terrain.transform.position.y : 0f;

        _currentAmmo = _maxAmmo;
        _shootTimer = _shootRate;

        if (_muzzleFire != null) _muzzleFire.Stop();
        if (_muzzleSmoke != null) _muzzleSmoke.Stop();
    }

    private void Start()
    {
        SetStatus(Status);
    }

    private void Update()
    {
        if (_target == Vector3.zero) return;

        _shootTimer += Time.deltaTime;
        HandleReload();
        AimAtTarget();
    }

    private void HandleReload()
    {
        if (_currentAmmo > 0) return;

        _reloadTimer += Time.deltaTime;
        if (_reloadTimer >= _reloadTime)
        {
            _currentAmmo = _maxAmmo;
            _reloadTimer = 0f;
        }
    }

    public void SetTarget(Vector3 target)
    {
        _target = target;
        _isAiming = false;
        SetStatus(PaladinStatus.Aiming);
    }

    private void AimAtTarget()
    {
        Vector3 flatTarget = _target;
        flatTarget.y = _tower.position.y;

        Quaternion towerYaw = Quaternion.LookRotation(flatTarget - _tower.position);
        _desiredTowerRotation = towerYaw;
        _tower.rotation = Quaternion.RotateTowards(
            _tower.rotation,
            towerYaw,
            _towerRotationSpeed * Time.deltaTime
        );

        if (!CalculateAngles(_target, _shootForce, out float low, out float high))
            return;

        float chosenAngle = ChooseBestAngle(low, high);
        chosenAngle = Mathf.Clamp(chosenAngle, _minElevationAngle, _maxElevationAngle);

        Quaternion gunPitch = Quaternion.Euler(-chosenAngle, 0f, 0f);
        _desiredGunRotation = gunPitch;
        _gun.localRotation = Quaternion.RotateTowards(
            _gun.localRotation,
            gunPitch,
            _gunRotationSpeed * Time.deltaTime
        );

        bool towerAligned = Quaternion.Angle(_tower.rotation, _desiredTowerRotation) < _aimThreshold;
        bool gunAligned   = Quaternion.Angle(_gun.localRotation, _desiredGunRotation) < _aimThreshold;

        if (!_isAiming && towerAligned && gunAligned)
        {
            _isAiming = true;
            SetStatus(PaladinStatus.Aimed);
            OnAimed?.Invoke();
        }

        if (_drawTrajectories)
            DrawTrajectory(chosenAngle, Color.green);
    }
    
    public float GetImpactRadius()
    {
        if (_target == Vector3.zero) return 0f;
        
        float dist = Vector3.Distance(_shootPoint.position, _target);
        float angle = Mathf.Max(_minSpreadAngle, _maxSpreadAngle);
        
        return dist * Mathf.Tan(angle * Mathf.Deg2Rad);
    }

    
    private bool IsTrajectoryClear(float angleDeg)
    {
        Vector3 pos = _shootPoint.position;
        Vector3 dirXZ = (_target - pos);
        dirXZ.y = 0f;
        dirXZ.Normalize();

        float a = angleDeg * Mathf.Deg2Rad;
        Vector3 v = dirXZ * (_shootForce * Mathf.Cos(a)) + Vector3.up * (_shootForce * Mathf.Sin(a));

        for (int i = 0; i < _maxSteps; i++)
        {
            Vector3 next = pos + v * _timeStep + 0.5f * Physics.gravity * _timeStep * _timeStep;

            if (Physics.Linecast(pos, next, out RaycastHit hit, _obstacleMask))
                return false;

            if (_terrain && next.y < _terrain.SampleHeight(next) + _terrainBaseY + 0.1f)
                return false;

            v += Physics.gravity * _timeStep;
            pos = next;
        }

        return true;
    }
    private float ChooseBestAngle(float low, float high)
    {
        Vector3 dirXZ = (_target - _shootPoint.position);
        dirXZ.y = 0f;
        dirXZ.Normalize();

        if (IsTrajectoryClear(low))
            return low;
        else
            return high; 
    }

    public void Fire()
    {
        if (_shootTimer < _shootRate || _currentAmmo <= 0) return;
        _shootTimer = 0f;
        _currentAmmo--;

        SetStatus(PaladinStatus.Fired);

        var bulletObj = Instantiate(_bulletPrefab, _shootPoint.position, _shootPoint.rotation);

        float yaw = 0f;
        float pitch = 0f;
        if (_useSpread && (_minSpreadAngle != 0f || _maxSpreadAngle != 0f))
        {
            float spread = Random.Range(_minSpreadAngle, _maxSpreadAngle);
            yaw = Random.Range(-spread, spread);
            pitch = Random.Range(-spread, spread);
        }

        bulletObj.transform.rotation *= Quaternion.Euler(-pitch, yaw, 0f);
        bulletObj.GetComponent<Bullet>().Shoot(_shootForce);
        

        if (_muzzleFire != null) _muzzleFire.Play();
        if (_muzzleSmoke != null) _muzzleSmoke.Play();

        StartCoroutine(RecoilRoutine());
        StartCoroutine(ReturnToIdleAfterDelay());

        OnFired?.Invoke();
    }

    private IEnumerator ReturnToIdleAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        SetStatus(PaladinStatus.Idle);
    }

    private IEnumerator RecoilRoutine()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * _gunRecoilSpeed;
            float recoil = Mathf.Sin(Mathf.PI * t) * _gunRecoilDistance;
            _cannonPivot.localPosition = _gunInitialLocalPos + Vector3.back * recoil;
            yield return null;
        }
        _cannonPivot.localPosition = _gunInitialLocalPos;
    }

    private bool CalculateAngles(Vector3 target, float v, out float low, out float high)
    {
        low = high = 0f;

        Vector3 d = target - _shootPoint.position;
        float xz = new Vector2(d.x, d.z).magnitude;
        float y = d.y;

        float g = Mathf.Abs(Physics.gravity.y);
        float v2 = v * v;

        float disc = v2 * v2 - g * (g * xz * xz + 2f * y * v2);
        if (disc < 0f) return false;

        float sq = Mathf.Sqrt(disc);

        low = Mathf.Rad2Deg * Mathf.Atan((v2 - sq) / (g * xz));
        high = Mathf.Rad2Deg * Mathf.Atan((v2 + sq) / (g * xz));
        return true;
    }

    private void DrawTrajectory(float angleDeg, Color col)
    {
        Vector3 pos = _shootPoint.position;
        Vector3 dirXZ = (_target - pos);
        dirXZ.y = 0f;
        dirXZ.Normalize();

        float a = angleDeg * Mathf.Deg2Rad;
        Vector3 v = dirXZ * (_shootForce * Mathf.Cos(a)) + Vector3.up * (_shootForce * Mathf.Sin(a));

        for (int i = 0; i < _maxSteps; i++)
        {
            Vector3 next = pos + v * _timeStep + 0.5f * Physics.gravity * _timeStep * _timeStep;
            Debug.DrawLine(pos, next, col, _drawInterval);

            if (_terrain && next.y < _terrain.SampleHeight(next) + _terrainBaseY + 0.1f)
                break;

            v += Physics.gravity * _timeStep;
            pos = next;
        }
    }
}
