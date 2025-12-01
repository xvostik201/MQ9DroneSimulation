using UnityEngine;

public static class TerrainDeformer
{
    public static void CreateCrater(Terrain terrain, Vector3 worldPoint, float craterRadius, float craterDepth, int craterLayerIndex)
    {
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
            if (terrain == null) return;
        }

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int hmResolution = terrainData.heightmapResolution;
        int amResolution = terrainData.alphamapResolution;

        float relativeX = (worldPoint.x - terrainPos.x) / terrainData.size.x;
        float relativeZ = (worldPoint.z - terrainPos.z) / terrainData.size.z;

        int centerX = Mathf.RoundToInt(relativeX * (hmResolution - 1));
        int centerZ = Mathf.RoundToInt(relativeZ * (hmResolution - 1));
        int radiusHM = Mathf.RoundToInt((craterRadius / terrainData.size.x) * hmResolution);

        int xStartHM = Mathf.Clamp(centerX - radiusHM, 0, hmResolution - 1);
        int zStartHM = Mathf.Clamp(centerZ - radiusHM, 0, hmResolution - 1);
        int xEndHM = Mathf.Clamp(centerX + radiusHM, 0, hmResolution - 1);
        int zEndHM = Mathf.Clamp(centerZ + radiusHM, 0, hmResolution - 1);

        int widthHM = xEndHM - xStartHM + 1;
        int heightHM = zEndHM - zStartHM + 1;

        if (terrain.gameObject.GetComponent<TerrainCollider>() != null)
        {
            float[,] heights = terrainData.GetHeights(xStartHM, zStartHM, widthHM, heightHM);
            float normalizedCraterDepth = craterDepth / terrainData.size.y;

            for (int z = 0; z < heightHM; z++)
            {
                for (int x = 0; x < widthHM; x++)
                {
                    int worldX = xStartHM + x;
                    int worldZ = zStartHM + z;

                    float dx = worldX - centerX;
                    float dz = worldZ - centerZ;
                    float distance = Mathf.Sqrt(dx * dx + dz * dz);

                    if (distance < radiusHM)
                    {
                        float influence = Mathf.Pow(1f - (distance / radiusHM), 2f);
                        heights[z, x] = Mathf.Clamp01(heights[z, x] - normalizedCraterDepth * influence);
                    }
                }
            }

            terrainData.SetHeights(xStartHM, zStartHM, heights);
        }

        int centerXAlpha = Mathf.RoundToInt(relativeX * (amResolution - 1));
        int centerZAlpha = Mathf.RoundToInt(relativeZ * (amResolution - 1));
        int radiusAlpha = Mathf.RoundToInt((craterRadius / terrainData.size.x) * amResolution);

        int xStartAlpha = Mathf.Clamp(centerXAlpha - radiusAlpha, 0, amResolution - 1);
        int zStartAlpha = Mathf.Clamp(centerZAlpha - radiusAlpha, 0, amResolution - 1);
        int xEndAlpha = Mathf.Clamp(centerXAlpha + radiusAlpha, 0, amResolution - 1);
        int zEndAlpha = Mathf.Clamp(centerZAlpha + radiusAlpha, 0, amResolution - 1);

        int widthAlpha = xEndAlpha - xStartAlpha + 1;
        int heightAlpha = zEndAlpha - zStartAlpha + 1;

        float[,,] alphamaps = terrainData.GetAlphamaps(xStartAlpha, zStartAlpha, widthAlpha, heightAlpha);

        for (int z = 0; z < heightAlpha; z++)
        {
            for (int x = 0; x < widthAlpha; x++)
            {
                int worldX = xStartAlpha + x;
                int worldZ = zStartAlpha + z;

                float dx = worldX - centerXAlpha;
                float dz = worldZ - centerZAlpha;
                float distance = Mathf.Sqrt(dx * dx + dz * dz);

                if (distance < radiusAlpha)
                {
                    float influence = Mathf.Pow(1f - (distance / radiusAlpha), 2f);

                    for (int i = 0; i < terrainData.alphamapLayers; i++)
                    {
                        if (i == craterLayerIndex)
                            alphamaps[z, x, i] = influence;
                        else
                            alphamaps[z, x, i] *= (1f - influence);
                    }
                }
            }
        }

        terrainData.SetAlphamaps(xStartAlpha, zStartAlpha, alphamaps);

        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider != null)
        {
            terrainCollider.enabled = false;
            terrainCollider.enabled = true;
        }
    }

}
