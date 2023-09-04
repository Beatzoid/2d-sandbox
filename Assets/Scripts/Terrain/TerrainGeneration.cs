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
    public int chunkSize = 16;
    public bool generateCaves = true;

    [Header("Tree Config")]

    public int treeSpawnChance = 10;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Ore Config")]
    public float coalRarity;
    // The inspector does weird things if coalRarity and coalSize are defined in the same line
    public float coalSize;
    public float ironRarity, ironSize;
    public float goldRarity, goldSize;
    public float diamondRarity, diamondSize;

    [Space(10)]

    public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;
    public Texture2D diamondSpread;

    [Header("Noise Config")]
    public float seed = 0f;
    public float caveFrequency = 0.05f;
    public float terrainFrequency = 0.05f;
    public Texture2D caveNoiseTexture;

    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;

    private List<Vector2> tilePositions = new();
    private GameObject[] worldChunks;
    private float gridOffset = 0.5f;

    public void OnValidate()
    {
        Init();
    }

    public void Start()
    {
        seed = Random.Range(-1000000, 1000000);

        Init();

        CreateChunks();
        GenerateTerrain();
    }

    public void CreateChunks()
    {
        // Calculate the number of chunks needed to cover the entire world
        // It does this by dividing the world size by the chunk size
        // and then adds one if there is a remainder, since we need an extra chunk
        // to cover the remaining tiles that didn't fit
        int numChunks = (worldSize / chunkSize) + ((worldSize % chunkSize == 0) ? 0 : 1);

        worldChunks = new GameObject[numChunks];

        for (int i = 0; i < numChunks; i++)
        {
            GameObject chunk = new GameObject(name = $"Chunk {i}");
            chunk.transform.parent = this.transform;

            worldChunks[i] = chunk;
        }
    }

    public void PlaceTile(Sprite tileSprite, int x, int y)
    {
        // Create a new tile object
        GameObject newTile = new();

        // Set the sprite to the tile
        newTile.AddComponent<SpriteRenderer>().sprite = tileSprite;

        // Set its position in the world, with an offset to align it to the grid
        newTile.transform.position = new Vector2(x + gridOffset, y + gridOffset);
        // Set the name of the tile to the name of the sprite
        newTile.transform.name = tileSprite.name;

        // Calculate the coordinate of the chunk that the new tile should be placed in based on its x position
        int chunkCoord = Mathf.FloorToInt(x / chunkSize) * chunkSize;

        // Divide the chunk coordinate by the chunk size to get the index of the corresponding chunk in the worldChunks array
        chunkCoord /= chunkSize;

        // Set the parent of the new tile to be the game object of the chunk it belongs to
        newTile.transform.parent = worldChunks[chunkCoord].transform;

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
                    tileSprite = tileAtlas.grass.tileSprite;
                }
                // Check if the tile is within the dirt layer
                else if (worldY > terrainHeight - dirtLayerHeight)
                {
                    // If it is, use the dirt sprite
                    tileSprite = tileAtlas.dirt.tileSprite;
                }
                // Otherwise, the tile is below the dirt layer
                else
                {
                    if (coalSpread.GetPixel(worldX, worldY).r > 0.5f)
                        tileSprite = tileAtlas.coal.tileSprite;
                    else if (ironSpread.GetPixel(worldX, worldY).r > 0.5f)
                        tileSprite = tileAtlas.iron.tileSprite;
                    else if (goldSpread.GetPixel(worldX, worldY).r > 0.5f)
                        tileSprite = tileAtlas.gold.tileSprite;
                    else if (diamondSpread.GetPixel(worldX, worldY).r > 0.5f)
                        tileSprite = tileAtlas.diamond.tileSprite;
                    else
                        tileSprite = tileAtlas.stone.tileSprite;
                }

                if (generateCaves)
                {
                    // Check if the current pixel value is greater than 0.5f
                    if (caveNoiseTexture.GetPixel(worldX, worldY).r > 0.5f)
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

    private void Init()
    {
        if (caveNoiseTexture == null)
        {
            caveNoiseTexture = new Texture2D(worldSize, worldSize);
            coalSpread = new Texture2D(worldSize, worldSize);
            ironSpread = new Texture2D(worldSize, worldSize);
            goldSpread = new Texture2D(worldSize, worldSize);
            diamondSpread = new Texture2D(worldSize, worldSize);
        }

        GenerateNoiseTexture(caveFrequency, surfaceThreshold, caveNoiseTexture);

        GenerateNoiseTexture(coalRarity, coalSize, coalSpread);
        GenerateNoiseTexture(ironRarity, ironSize, ironSpread);
        GenerateNoiseTexture(goldRarity, goldSize, goldSpread);
        GenerateNoiseTexture(diamondRarity, diamondSize, diamondSpread);
    }

    private void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                // Generate noise value based on x, seed and noiseFrequency
                float noiseValue = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                if (noiseValue > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }

        noiseTexture.Apply();
    }

    private void GenerateTree(int x, int y)
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
            PlaceTile(tileAtlas.treeLog.tileSprite, x, y + i);
        }

        // Tree leaves (a, b, c)

        // Center column of leaves (c)
        PlaceTile(tileAtlas.treeLeaf.tileSprite, x, y + treeHeight);
        PlaceTile(tileAtlas.treeLeaf.tileSprite, x, y + treeHeight + 1);
        PlaceTile(tileAtlas.treeLeaf.tileSprite, x, y + treeHeight + 2);

        // Side columns of leaves (b)
        PlaceTile(tileAtlas.treeLeaf.tileSprite, x - 1, y + treeHeight);
        PlaceTile(tileAtlas.treeLeaf.tileSprite, x - 1, y + treeHeight + 1);

        PlaceTile(tileAtlas.treeLeaf.tileSprite, x + 1, y + treeHeight);
        PlaceTile(tileAtlas.treeLeaf.tileSprite, x + 1, y + treeHeight + 1);

        // Leaves to left and right of side column (a)
        PlaceTile(tileAtlas.treeLeaf.tileSprite, x + 2, y + treeHeight);
        PlaceTile(tileAtlas.treeLeaf.tileSprite, x - 2, y + treeHeight);
    }
}
