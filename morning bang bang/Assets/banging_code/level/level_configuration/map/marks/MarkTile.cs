using UnityEngine.Tilemaps;

namespace banging_code.common.rooms
{
    public abstract class MarkTile : Tile
    {
        public abstract TileBase GetTile();
    }
}