using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.common
{
    public static class TilemapExtensions 
    {
        //return all tiles and their positions on tilemap with TTile type
        public static List<TilePT<TTile>> FindTilesOfType<TTile>(this Tilemap tilemap)
            where TTile : Tile
        {
            tilemap.CompressBounds();
            List<TilePT<TTile>> result = new List<TilePT<TTile>>();
            
            for(int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
            {
                for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++)
                {
                    var tile = tilemap.GetTile<TTile>(new Vector3Int(x, y, 0));
                    
                    if (tile != null)
                    {
                        result.Add(new TilePT<TTile>(new Vector3Int(x, y), tile));
                    }
                }   
            }

            return result;
        }
        
        //will expand each side on n cells
        public static void Expand(this Tilemap tilemap, int n)
        {
            tilemap.size   += new Vector3Int(n * 2,n * 2);    //adding n cells to top and to right
            tilemap.origin -= new Vector3Int(n, n);           //moving tilemap "center" on n cells down and left

            tilemap.ResizeBounds();
        }   
        
        //all tiles from FROM TILEMAP will be added to TO TILEMAP with saving the position
        public static void CopyTilesTo(this Tilemap from, Tilemap to, Func<Vector3Int, bool> processTile = null)
         {
             for (int x = (int)from.localBounds.min.x; x < from.localBounds.max.x; x++)
             {
                 for (int y = (int)from.localBounds.min.y; y < from.localBounds.max.y; y++)
                 {
                     Vector3Int originalPosition = new Vector3Int(x, y, 0);
                     
                     if (!from.HasTile(originalPosition)) continue;
     
                     if (processTile != null && !processTile.Invoke(originalPosition))
                     {
                         continue;
                     }
     
                     var copyPosition = to.WorldToCell(from.CellToWorld(originalPosition));
                     to.SetTile(copyPosition, from.GetTile(originalPosition));
                 }
             }
         }
    }
}