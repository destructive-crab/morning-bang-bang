using banging_code.common.rooms;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.level.level_configuration
{
    [CreateAssetMenu(menuName = "Tiles/Floor Mark Tile", fileName = "Floor Mark Tile")]
    public class FloorMark : MarkTile
    {
        [SerializeField] private FloorTile Tile;
        
        public override TileBase GetTile()
        {
            return Tile;
        }
    }
}