using banging_code.level.structure.map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.level.structure
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