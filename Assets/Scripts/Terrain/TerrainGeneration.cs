using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Generation Config")]

    public int worldSize = 100;
    public float surfaceThreshold = 0.25f;
    public int dirtLayerHeight = 5;
    public float heightMultiplier = 4f;
    public int heightAddition = 25;
    public bool generateCaves = true;

    [Header("Tree Config")]

    public int treeSpawnChance = 10;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Noise Settings")]
    public float seed = 0f;
    public float caveFrequency = 0.05f;
    public float terrainFrequency = 0.05f;
    public Texture2D noiseTexture;

    [Header("Terrain Art")]

    public Sprite grass;
    public Sprite dirt;
    public Sprite stone;
    public Sprite treeLog;
    public Sprite treeLeaf;

    private List<Vector2> tilePositions = new();
    private float gridOffset = 0.5f;

    public void Start()
    {
        seed = Random.Range(-1000000, 1000000);

        GenerateNoiseTexture();
        GenerateTerrain();
    }

    public void PlaceTile(Sprite tileSprite, float x, float y)
    {
        // Create a new tile object
        GameObject newTile = new();

        // Set the sprite to the tile
        newTile.AddComponent<SpriteRenderer>().sprite = tileSprite;

        // Set its position in the world, with an offset to align it to the grid
        newTile.transform.position = new Vector2(x + gridOffset, y + gridOffset);
        // Set parent to the Terrain object
        newTile.transform.parent = transform;
        // Set the name of the tile to the name of the sprite
        newTile.transform.name = tileSprite.name;

        // Add the tile position (with the grid offset) to the global list of all tile positions
        tilePositions.Add(newTile.transform.position - (Vector3.one * gridOffset));
    }

    public void GenerateTerrain()
    {
        for (int worldX = 0; worldX < worldSize; worldX++)
        {
            // Calculate terrain height using Perlin noise
            float perlinValue = Mathf.PerlinNoise((worldX + seed) * terrainFrequency, seed * terrainFrequency);
            float terrainHeight = (perlinValue * heightMultiplier) + heightAddition;

            for (int worldY = 0; worldY < terrainHeight; worldY++)
            {
                Sprite tileSprite;

                // Check if the tile is above the dirt layer (aka if it is the top layer)
                if (worldY > terrainHeight - 1)
                {
                    // If it is, use the grass sprite
                    tileSprite = grass;
                }
                // Check if the tile is within the dirt layer
                else if (worldY > terrainHeight - dirtLayerHeight)
                {
                    // If it is, use the dirt sprite
                    tileSprite = dirt;
                }
                // Otherwise, the tile is below the dirt layer
                else
                {
                    // Use the stone sprite
                    tileSprite = stone;
                }

                if (generateCaves)
                {
                    // Check if the current pixel value is greater than the surface threshold
                    if (noiseTexture.GetPixel(worldX, worldY).r > surfaceThreshold)
                    {
                        PlaceTile(tileSprite, worldX, worldY);
                    }
                }
                else
                {
                    PlaceTile(tileSprite, worldX, worldY);
                }

                if (worldY >= terrainHeight - 1)
                {
                    int treeChance = Random.Range(0, treeSpawnChance);

                    if (treeChance == 1)
                    {
                        // Check if there is a tile under where we want to generate a tree
                        if (tilePositions.Contains(new Vector2(worldX, worldY)))
                        {
                            // Generate a tree, with a + 1 vertical offset to place it
                            // on top of the grass and not inside it
                            GenerateTree(worldX, worldY + 1);
                        }
                    }
                }
            }
        }
    }

    private void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(worldSize, worldSize);

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                // Generate noise value based on x, seed and noiseFrequency
                float noiseValue = Mathf.PerlinNoise((x + seed) * caveFrequency, (y + seed) * caveFrequency);

                // Set the pixel color to the generated noise color
                noiseTexture.SetPixel(x, y, new Color(noiseValue, noiseValue, noiseValue));
            }
        }

        noiseTexture.Apply();
    }

    private void GenerateTree(float x, float y)
    {
        // Generate a random tree height, with + 1 on the max because it's exclusive
        int treeHeight = Random.Range(minTreeHeight, maxTreeHeight + 1);

        /**

        Tree structure:
          c
         bcb
        abcba
          d
          d
          d
          d
        **/

        // Tree logs (d)
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(treeLog, x, y + i);
        }

        // Tree leaves (a, b, c)

        // Center column of leaves (c)
        PlaceTile(treeLeaf, x, y + treeHeight);
        PlaceTile(treeLeaf, x, y + treeHeight + 1);
        PlaceTile(treeLeaf, x, y + treeHeight + 2);

        // Side columns of leaves (b)
        PlaceTile(treeLeaf, x - 1, y + treeHeight);
        PlaceTile(treeLeaf, x - 1, y + treeHeight + 1);

        PlaceTile(treeLeaf, x + 1, y + treeHeight);
        PlaceTile(treeLeaf, x + 1, y + treeHeight + 1);

        // Leaves to left and right of side column (a)
        PlaceTile(treeLeaf, x + 2, y + treeHeight);
        PlaceTile(treeLeaf, x - 2, y + treeHeight);
    }
}
