using UnityEngine;

[CreateAssetMenu(fileName = "Tile Atlas", menuName = "Tile/Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    public Tile grass;
    public Tile dirt;
    public Tile stone;
    public Tile treeLog;
    public Tile treeLeaf;
    public Tile tallGrass;

    [Space(10)]

    public Tile coal;
    public Tile iron;
    public Tile gold;
    public Tile diamond;
}
