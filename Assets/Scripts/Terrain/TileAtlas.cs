using UnityEngine;

[CreateAssetMenu(fileName = "Tile Atlas", menuName = "Tile/Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Environment")]
    public Tile grass;
    public Tile dirt;
    public Tile stone;
    public Tile treeLog;
    public Tile treeLeaf;
    public Tile tallGrass;
    public Tile snow;
    public Tile sand;

    [Header("Ores")]

    public Tile coal;
    public Tile iron;
    public Tile gold;
    public Tile diamond;
}
