using UnityEngine.Tilemaps;

namespace banging_code.level.structure.map
{
    public interface IObstacleTile 
    {
        TileBase This();
    }
    public interface IFloorTile
    {
        bool IsWalkable();
        TileBase This();
    }
}