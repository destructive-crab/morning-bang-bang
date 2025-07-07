using banging_code.level.structure.map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.level.structure
{
    [CreateAssetMenu(menuName = "Tiles/Rule Floor", fileName = "Floor Tile")]
    public class RuleFloorTile : RuleTile, IFloorTile
    {
        public bool IsWalkable()
        {
            return true;
        }

        public TileBase This() => this;
    }
}