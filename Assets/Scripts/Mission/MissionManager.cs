using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [Header("Mission Data")]
    [SerializeField] private MissionDatabase _database;
    [SerializeField] private MissionSpawner _spawner;

    private MissionData _missionData;
    private List<Health> _enemies = new();

    public MissionData MissionData => _missionData;

    public event Action OnMissionStarted;
    public event Action OnAllEnemiesDied;
    public event Action OnMissionCompleted;
    public event Action<int> OnEnemyKilled;

    private bool _started;
    private bool _completed;

    private void Start()
    {
        LoadCurrentMission();
        SpawnMissionContent();
        StartMission();
    }

    private void LoadCurrentMission()
    {
        int index = ProgressManager.Data.CurrentMission;

        if (index >= _database.missions.Length)
        {
            SceneLoader.Load("CreditsScene");
            return;
        }

        _missionData = _database.missions[index];
    }

    private void SpawnMissionContent()
    {
        _spawner.SpawnMission(_missionData.missionPrefab);
        _enemies = _spawner.GetEnemies();
    }

    public void StartMission()
    {
        if (_started) return;
        _started = true;

        RegisterEnemies();
        OnMissionStarted?.Invoke();
    }

    private void RegisterEnemies()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy == null) continue;
            var e = enemy;

            e.OnDeath += () => HandleEnemyDeath(e);
        }
    }

    private void HandleEnemyDeath(Health enemy)
    {
        if (_completed) return;

        int deadCount = _enemies.Count(e => e == null || e.IsDead);
        OnEnemyKilled?.Invoke(deadCount);

        if (_enemies.All(e => e == null || e.IsDead))
        {
            OnAllEnemiesDied?.Invoke();
            CompleteMission();
        }
    }

    public void CompleteMission()
    {
        if (_completed) return;
        _completed = true;

        ProgressManager.Data.CurrentMission++;
        ProgressManager.Save();

        OnMissionCompleted?.Invoke();
    }

    public void ResetMission()
    {
        _completed = false;
        _started = false;

        _spawner.Clear();
    }

    public void LaunchNextMission()
    {
        LoadCurrentMission();
        SpawnMissionContent();
        StartMission();
    }
}
