using UnityEngine;

public class SnowDeformByStep : MonoBehaviour
{
    public Terrain terrain;
    public float stepThreshold = 1f; // Distance the player must move before another deformation is applied
    public int brushSize = 1;
    public float opacity = 0.5f;
    public int textureLayer = 1; // Index of the texture to apply

    private Vector3 lastStepPosition;
    private TerrainData terrainData;
    private float[,] originalHeights;
    private bool[,] deformedPoints;
    private float[,,] originalSplatmapData;

    void Start()
    {
        terrainData = terrain.terrainData;
        originalHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        deformedPoints = new bool[terrainData.heightmapResolution, terrainData.heightmapResolution];
        originalSplatmapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        lastStepPosition = transform.position;
    }

    void Update()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastStepPosition);
        if (distanceMoved >= stepThreshold)
        {
            DeformTerrainUnderPlayer();
            lastStepPosition = transform.position;
        }
    }

    void DeformTerrainUnderPlayer()
    {
        Vector3 terrainPosition = transform.position - terrain.transform.position;
        int mapX = (int)(terrainPosition.x / terrainData.size.x * terrainData.heightmapResolution);
        int mapZ = (int)(terrainPosition.z / terrainData.size.z * terrainData.heightmapResolution);

        int brushStartX = Mathf.Clamp(mapX - brushSize / 2, 0, terrainData.heightmapResolution - 1);
        int brushStartZ = Mathf.Clamp(mapZ - brushSize / 2, 0, terrainData.heightmapResolution - 1);
        int brushWidth = Mathf.Min(brushSize, terrainData.heightmapResolution - brushStartX);
        int brushHeight = Mathf.Min(brushSize, terrainData.heightmapResolution - brushStartZ);

        float[,] heights = terrainData.GetHeights(brushStartX, brushStartZ, brushWidth, brushHeight);

        float[,,] splatmapData = terrainData.GetAlphamaps(brushStartX, brushStartZ, brushWidth, brushHeight);

        for (int x = 0; x < brushWidth; x++)
        {
            for (int z = 0; z < brushHeight; z++)
            {
                if (!deformedPoints[brushStartX + x, brushStartZ + z])
                {
                    heights[x, z] -= opacity * 0.001f; // Adjust this factor based on desired depth per step

                    // Reset other textures' influence
                    for (int i = 0; i < terrainData.alphamapLayers; i++)
                    {
                        splatmapData[x, z, i] = 0;
                    }

                    // Apply the chosen texture
                    splatmapData[x, z, textureLayer] = 1;

                    deformedPoints[brushStartX + x, brushStartZ + z] = true;
                }
            }
        }

        terrainData.SetHeights(brushStartX, brushStartZ, heights);
        terrainData.SetAlphamaps(brushStartX, brushStartZ, splatmapData);
    }

    void OnDisable()
    {
        // Reset the terrain to its original state when the script is disabled or the object is destroyed
        terrainData.SetHeights(0, 0, originalHeights);
        terrainData.SetAlphamaps(0, 0, originalSplatmapData);
    }
}
