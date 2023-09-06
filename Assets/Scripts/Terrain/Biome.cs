using UnityEngine;

[System.Serializable]
public class Biome
{
    public string biomeName;
    public Color biomeColor;

    [Header("Noise Config")]
    public float caveFrequency = 0.05f;
    public float terrainFrequency = 0.05f;
    public Texture2D caveNoiseTexture;

    [Header("Terrain Config")]

    public int dirtLayerHeight = 5;
    public float surfaceThreshold = 0.25f;
    public float heightMultiplier = 4f;
    public bool generateCaves = true;

    [Header("Tree Config")]

    public int treeSpawnChance = 10;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Addons")]

    public int tallGrassChance = 10;

    [Header("Ore Config")]

    public Ore[] ores;
}
