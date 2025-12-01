using UnityEngine;
using System.Collections.Generic;

public class MissionSpawner : MonoBehaviour
{
    private GameObject _spawnedMission;

    public GameObject SpawnMission(GameObject prefab)
    {
        Clear();

        _spawnedMission = Instantiate(prefab);
        return _spawnedMission;
    }

    public List<Health> GetEnemies()
    {
        if (_spawnedMission == null)
            return new List<Health>();

        return new List<Health>(_spawnedMission.GetComponentsInChildren<Health>(true));
    }

    public void Clear()
    {
        if (_spawnedMission != null)
            Destroy(_spawnedMission);
    }
}