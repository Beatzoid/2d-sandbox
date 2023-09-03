using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Terrain Config")]
    public int worldSize = 100;
    public float caveFrequency = 0.05f;
    public float terrainFrequency = 0.05f;
    public float surfaceValue = 0.25f;

    [Range(-1000000, 1000000)]
    public float seed = 0f;
    public float heightMultiplier = 4f;
    public int heightAddition = 25;

    [Header("Terrain Art")]
    public Sprite tile;

    [Header("Internal - Don't modify")]
    public Texture2D noiseTexture;

    public void Start()
    {
        seed = Random.Range(-1000000, 1000000);

        GenerateNoiseTexture();
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        for (int x = 0; x < worldSize; x++)
        {
            float height =
                (Mathf.PerlinNoise((x + seed) * terrainFrequency, seed * terrainFrequency) * heightMultiplier) + heightAddition;

            for (int y = 0; y < height; y++)
            {
                // Generates caves
                if (noiseTexture.GetPixel(x, y).r > surfaceValue)
                {
                    // Create new tile object
                    GameObject newTile = new(name = $"Tile ({x}, {y})");

                    // Set the sprite to the tile
                    newTile.AddComponent<SpriteRenderer>().sprite = tile;

                    // Set its position in the world, with 0.5f offset to align it to the grid
                    newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
                    // Set parent to the Terrain object
                    newTile.transform.parent = transform;
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
                // Generate noise color based on x, seed and noiseFrequency
                float v = Mathf.PerlinNoise((x + seed) * caveFrequency, (y + seed) * caveFrequency);

                // Set the pixel color to the generated noise color
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }

        noiseTexture.Apply();
    }
}
