using banging_code.level.level_configuration.map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.level.level_configuration
{
    [CreateAssetMenu(menuName = "Tiles/Floor", fileName = "Floor Tile")]
    public class FloorTile : Tile, IFloorTile
    {
        public bool IsWalkable()
        {
            return true;
        }

        public TileBase This() => this;
    }
}