using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MenuDronBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _propeller;
    [SerializeField] private Transform[] _wheels;
    [SerializeField] private Light _whiteBody;
    [SerializeField] private Light _redLeft;
    [SerializeField] private Light _greenRight;

    [Header("Engine Audio (Low & High)")]
    [SerializeField] private AudioSource _engineLow;
    [SerializeField] private AudioSource _engineHigh;
    [SerializeField] private float _lowMaxVolume = 0.45f;
    [SerializeField] private float _highMaxVolume = 0.35f;
    [SerializeField] private float _highStartThreshold = 0.35f;
    [SerializeField] private float _highPitchMax = 1.25f;

    [Header("Engine Behaviour")]
    [SerializeField] private float _spinUpTime = 4f;
    [SerializeField] private float _maxPropellerRPM = 1500f;
    [SerializeField] private AnimationCurve _engineCurve;

    [Header("Taxi")]
    [SerializeField] private float _driveDuration = 3.5f;
    [SerializeField] private float _taxiStartThreshold = 0.7f;

    private float _whiteLightIntensity;
    private float _redLightIntensity;
    private float _greenLightIntensity;

    private float _enginePower;
    private float _engineTimer;
    private float _wheelSpeed;
    private bool _isTaxiing;

    private void Awake()
    {
        _whiteLightIntensity = _whiteBody.intensity;
        _redLightIntensity = _redLeft.intensity;
        _greenLightIntensity = _greenRight.intensity;

        _whiteBody.intensity = _redLeft.intensity = _greenRight.intensity = 0;

        _engineLow.volume = 0f;
        _engineHigh.volume = 0f;
    }

    private void Start()
    {
        _engineLow.clip = AudioManager.Instance.Get(SoundID.DroneEngine);
        _engineHigh.clip = AudioManager.Instance.Get(SoundID.DroneWhine);
    }

    public void StartSequence()
    {
        StartCoroutine(StartupRoutine());
    }

    private void Update()
    {
        if (_enginePower > 0f)
        {
            float rpm = _maxPropellerRPM * _enginePower;
            _propeller.Rotate(Vector3.forward, rpm * Time.deltaTime);
        }

        if (_isTaxiing)
        {
            foreach (Transform wh in _wheels)
                wh.Rotate(Vector3.right * Time.deltaTime * _wheelSpeed);
        }
    }

    private IEnumerator StartupRoutine()
    {
        _greenRight.DOIntensity(_greenLightIntensity, 2f);
        _redLeft.DOIntensity(_redLightIntensity, 2f);
        _whiteBody.DOIntensity(_whiteLightIntensity, 1.5f);

        _engineLow.Play();
        _engineHigh.Play();

        _engineTimer = 0f;

        while (_engineTimer < _spinUpTime)
        {
            _engineTimer += Time.deltaTime;
            float t = _engineTimer / _spinUpTime;
            _enginePower = _engineCurve.Evaluate(t);

            UpdateEngineAudio();
            yield return null;
        }

        _enginePower = 1f;

        yield return new WaitForSeconds(0.4f);

        StartCoroutine(TaxiRoutine());
    }

    private void UpdateEngineAudio()
    {
        _engineLow.volume = Mathf.Lerp(0f, _lowMaxVolume, _enginePower / 2);

        if (_enginePower > _highStartThreshold)
        {
            float turb = Mathf.InverseLerp(_highStartThreshold, 1f, _enginePower);
            _engineHigh.volume = Mathf.Lerp(0f, _highMaxVolume, turb / 2);
            _engineHigh.pitch = Mathf.Lerp(1f, _highPitchMax, turb);
        }
        else
            _engineHigh.volume = 0f;
    }

    private IEnumerator TaxiRoutine()
    {
        float timer = 0f;
        float loadDelay = 5f;
        bool loadStarted = false;

        while (_enginePower < _taxiStartThreshold)
            yield return null;

        _isTaxiing = true;
        float moveSpeed = 0.4f;
        _wheelSpeed = 250f;

        while (true)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
            timer += Time.deltaTime;

            if (!loadStarted && timer >= loadDelay)
            {
                loadStarted = true;
                SceneLoader.Load("GameScene");
            }

            yield return null;
        }
    }
}
