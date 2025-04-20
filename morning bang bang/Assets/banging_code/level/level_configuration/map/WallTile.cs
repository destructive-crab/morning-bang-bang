using banging_code.level.structure.map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.level
{
    [CreateAssetMenu(menuName = "Tiles/Wall Tile", fileName = "Wall Tile")]
    public class WallTile : RuleTile, IObstacleTile
    {
        public TileBase This() => this;
    }
}