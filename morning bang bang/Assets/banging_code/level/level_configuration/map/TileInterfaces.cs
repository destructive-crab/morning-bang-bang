using UnityEngine.Tilemaps;

namespace banging_code.level.level_configuration.map
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