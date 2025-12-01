using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PaladinManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DroneController _drone;
    [SerializeField] private GameController _gameController;
    [SerializeField] private MapHUD _mapHUD;

    [SerializeField] private float _maxRangeToTarget = 1500f;
    [SerializeField] private int _maxPaladinsToUse = 3;

    [Header("Delays")]
    [SerializeField] private float _aimDelay = 0.5f;
    [SerializeField] private float _fireDelayBetweenGuns = 0.3f;

    private List<Paladin> _paladins = new();
    private Vector3? _lastTarget;
    private List<Paladin> _selectedPaladins = new();
    private PaladinStatus _currentGroupStatus = PaladinStatus.Idle;
    
    public event Action<Vector3> OnPaladinInit;
    public event Action<PaladinStatus> OnGroupStatusChanged;



    private void Awake()
    {
        if (_gameController != null)
            _gameController.OnMarkPlaced += HandleMarkPlaced;

        if (_drone != null)
            _drone.OnFireConfirmed += HandleFireConfirmed;
    }

    private void Start()
    {
    }
    
    public void RegisterPaladin(Paladin paladin)
    {
        if (!_paladins.Contains(paladin))
        {
            _paladins.Add(paladin);
            paladin.OnStatusChanged += HandlePaladinStatusChanged;
            OnPaladinInit?.Invoke(paladin.transform.position);

            _currentGroupStatus = EvaluateGroupStatus();
            OnGroupStatusChanged?.Invoke(_currentGroupStatus);
            
            paladin.OnAimed += () =>
            {
                float r = paladin.GetImpactRadius();
                _mapHUD.ShowImpactZone(paladin.TargetPosition, r);
            };

            paladin.OnFired += () =>
            {
                _mapHUD.HideImpactZone();
            };

        }
    }
    public void DestroyAllPaladins()
    {
        foreach (var pal in _paladins)
        {
            if (pal != null)
                Destroy(pal.gameObject);
        }

        _paladins.Clear();
        _selectedPaladins.Clear();
        _currentGroupStatus = PaladinStatus.Idle;

        OnGroupStatusChanged?.Invoke(PaladinStatus.Idle);

        Debug.Log("<color=cyan>[PaladinManager]</color> Все паладины уничтожены.");
    }


    private void OnDestroy()
    {
        if (_gameController != null)
            _gameController.OnMarkPlaced -= HandleMarkPlaced;

        if (_drone != null)
            _drone.OnFireConfirmed -= HandleFireConfirmed;
    }
    private void HandlePaladinStatusChanged(Paladin paladin, PaladinStatus status)
    {
        PaladinStatus groupStatus = EvaluateGroupStatus();

        if (groupStatus != _currentGroupStatus)
        {
            _currentGroupStatus = groupStatus;
            OnGroupStatusChanged?.Invoke(groupStatus);
        }
    }

    private PaladinStatus EvaluateGroupStatus()
    {
        if (_paladins.Any(p => p.Status == PaladinStatus.Fired))
            return PaladinStatus.Fired;

        if (_paladins.All(p => p.Status == PaladinStatus.Aimed))
            return PaladinStatus.Aimed;

        if (_paladins.Any(p => p.Status == PaladinStatus.Aiming))
            return PaladinStatus.Aiming;

        return PaladinStatus.Idle;
    }

    private void HandleMarkPlaced(Vector3 worldPoint)
    {
        _lastTarget = worldPoint;

        _selectedPaladins = _paladins
            .Where(p => p != null)
            .OrderBy(p => Vector3.Distance(p.transform.position, worldPoint))
            .Take(_maxPaladinsToUse)
            .ToList();

        if (_selectedPaladins.Count == 0)
        {
            Debug.Log("<color=yellow>[PaladinManager]</color> Нет доступных паладинов поблизости!");
            return;
        }

        foreach (var pal in _selectedPaladins)
            pal.SetTarget(worldPoint);

        Debug.Log($"<color=cyan>[PaladinManager]</color> Метка получена: {_selectedPaladins.Count} паладинов готовятся.");
    }

    private void HandleFireConfirmed()
    {
        if (!_lastTarget.HasValue)
        {
            Debug.Log("<color=yellow>[PaladinManager]</color> Нет цели для стрельбы!");
            return;
        }

        if (_selectedPaladins == null || _selectedPaladins.Count == 0)
        {
            Debug.Log("<color=yellow>[PaladinManager]</color> Нет выбранных паладинов!");
            return;
        }

        Debug.Log($"<color=red>[PaladinManager]</color> Приказ на огонь подтверждён. Цель: {_lastTarget.Value}");
        StartCoroutine(FireSequence(_selectedPaladins, _lastTarget.Value));
    }

    private IEnumerator FireSequence(List<Paladin> paladins, Vector3 target)
    {
        yield return new WaitForSeconds(_aimDelay);

        foreach (var pal in paladins)
        {
            if (pal == null) continue;

            bool ready = false;
            System.Action onAimed = () => ready = true;
            pal.OnAimed += onAimed;

            float timeout = 1f;
            float elapsed = 0f;
            while (!ready && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            pal.OnAimed -= onAimed;

            pal.Fire();
            yield return new WaitForSeconds(_fireDelayBetweenGuns);
        }
    }
}
