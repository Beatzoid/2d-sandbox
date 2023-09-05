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

    [Header("Addons")]

    public int tallGrassChance = 10;

    [Header("Ore Config")]
    public Ore[] ores;

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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            seed = Random.Range(-1000000, 1000000);
            Debug.Log(seed);

            // Destroy all chunks
            foreach (GameObject chunk in worldChunks)
            {
                Destroy(chunk);
            }

            // Clear the list of tile positions
            tilePositions.Clear();

            Init();

            // Create new chunks
            CreateChunks();

            // Generate new terrain
            GenerateTerrain();
        }
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

    public void PlaceTile(Sprite[] tileSprites, int x, int y)
    {
        // Create a new tile object
        GameObject newTile = new();

        // Get a random sprite from the tileSprites array
        int spriteIndex = Random.Range(0, tileSprites.Length);

        // Set the sprite to the tile
        newTile.AddComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];

        // Set its position in the world, with an offset to align it to the grid
        newTile.transform.position = new Vector2(x + gridOffset, y + gridOffset);
        // Set the name of the tile to the name of the sprite
        newTile.transform.name = tileSprites[spriteIndex].name;

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
                Sprite[] tileSprites;

                // Check if the tile is above the dirt layer (aka if it is the top layer)
                if (worldY > terrainHeight - 1)
                {
                    // If it is, use the grass sprite
                    tileSprites = tileAtlas.grass.tileSprites;
                }
                // Check if the tile is within the dirt layer
                else if (worldY > terrainHeight - dirtLayerHeight)
                {
                    // If it is, use the dirt sprite
                    tileSprites = tileAtlas.dirt.tileSprites;
                }
                // Otherwise, the tile is below the dirt layer
                else
                {
                    tileSprites = tileAtlas.stone.tileSprites;

                    if (ores[0].spreadTexture.GetPixel(worldX, worldY).r > 0.5f && terrainHeight - worldY > ores[0].minDistanceFromSurfaceToGenerate)
                        tileSprites = tileAtlas.coal.tileSprites;

                    if (ores[1].spreadTexture.GetPixel(worldX, worldY).r > 0.5f && terrainHeight - worldY > ores[1].minDistanceFromSurfaceToGenerate)
                        tileSprites = tileAtlas.iron.tileSprites;

                    if (ores[2].spreadTexture.GetPixel(worldX, worldY).r > 0.5f && terrainHeight - worldY > ores[2].minDistanceFromSurfaceToGenerate)
                        tileSprites = tileAtlas.gold.tileSprites;

                    if (ores[3].spreadTexture.GetPixel(worldX, worldY).r > 0.5f && terrainHeight - worldY > ores[3].minDistanceFromSurfaceToGenerate)
                        tileSprites = tileAtlas.diamond.tileSprites;
                }

                if (generateCaves)
                {
                    // Check if the current pixel value is greater than 0.5f
                    if (caveNoiseTexture.GetPixel(worldX, worldY).r > 0.5f)
                    {
                        PlaceTile(tileSprites, worldX, worldY);
                    }
                }
                else
                {
                    PlaceTile(tileSprites, worldX, worldY);
                }

                // If the tile is at the top of the terrain
                if (worldY >= terrainHeight - 1)
                {
                    int treeChance = Random.Range(0, treeSpawnChance + 1);

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
                    else
                    {
                        // Grass Generation

                        int i = Random.Range(0, tallGrassChance + 1);

                        if (i == 1 && tilePositions.Contains(new Vector2(worldX, worldY)))
                        {
                            PlaceTile(tileAtlas.tallGrass.tileSprites, worldX, worldY + 1);
                        }
                    }
                }
            }
        }
    }

    private void Init()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);

        for (int i = 0; i < ores.Length; i++)
        {
            ores[i].spreadTexture = new Texture2D(worldSize, worldSize);
        }

        GenerateNoiseTexture(caveFrequency, surfaceThreshold, caveNoiseTexture);

        for (int i = 0; i < ores.Length; i++)
        {
            GenerateNoiseTexture(ores[i].rarity, ores[i].size, ores[i].spreadTexture);
        }
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
            PlaceTile(tileAtlas.treeLog.tileSprites, x, y + i);
        }

        // Tree leaves (a, b, c)

        // Center column of leaves (c)
        PlaceTile(tileAtlas.treeLeaf.tileSprites, x, y + treeHeight);
        PlaceTile(tileAtlas.treeLeaf.tileSprites, x, y + treeHeight + 1);
        PlaceTile(tileAtlas.treeLeaf.tileSprites, x, y + treeHeight + 2);

        // Side columns of leaves (b)
        PlaceTile(tileAtlas.treeLeaf.tileSprites, x - 1, y + treeHeight);
        PlaceTile(tileAtlas.treeLeaf.tileSprites, x - 1, y + treeHeight + 1);

        PlaceTile(tileAtlas.treeLeaf.tileSprites, x + 1, y + treeHeight);
        PlaceTile(tileAtlas.treeLeaf.tileSprites, x + 1, y + treeHeight + 1);

        // Leaves to left and right of side column (a)
        PlaceTile(tileAtlas.treeLeaf.tileSprites, x + 2, y + treeHeight);
        PlaceTile(tileAtlas.treeLeaf.tileSprites, x - 2, y + treeHeight);
    }
}
