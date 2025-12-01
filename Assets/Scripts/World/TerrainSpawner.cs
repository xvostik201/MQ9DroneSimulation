using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _terrainPrefab;

    private void Awake()
    {
        GameObject terrainInstance = Instantiate(_terrainPrefab);

        Terrain originalTerrain = _terrainPrefab.GetComponent<Terrain>();
        Terrain clonedTerrain = terrainInstance.GetComponent<Terrain>();
        TerrainCollider clonedCollider = terrainInstance.GetComponent<TerrainCollider>();

        if (originalTerrain != null && clonedTerrain != null && clonedCollider != null)
        {
            TerrainData originalData = originalTerrain.terrainData;

            TerrainData newTerrainData = Instantiate(originalData);

            clonedTerrain.terrainData = newTerrainData;
            clonedCollider.terrainData = newTerrainData;
        }
    }
}
