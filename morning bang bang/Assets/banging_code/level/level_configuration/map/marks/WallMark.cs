using banging_code.level;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.common.rooms
{
    [CreateAssetMenu(menuName = "Tiles/Wall Mark Tile", fileName = "Wall Mark Tile")]
    public class WallMark : MarkTile
    {
        [SerializeField] private WallTile Tile;
        
        public override TileBase GetTile()
        {
           return Tile; 
        }
    }
}