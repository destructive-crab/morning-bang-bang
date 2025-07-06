using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MohDIed.Tilemaps
{
    public static class TilemapExtensions 
    {
        public static bool HasTile<TTile>(this Tilemap tilemap, Vector3Int position)
            where TTile : TileBase =>
            tilemap.GetTile<TTile>(position) != null;

        //                                                                  we do not give tilemap as parameter in func because
        //                                                                  TILEMAP SHOULD NOT BE edited in rule func.
        //                                                                  You should use rules only for some checks like to check some parameters in custom tile
        public static void Replace(this Tilemap inTilemap, TileBase toReplace, TileBase replacement, Func<Vector3Int, TileBase, TileBase, bool>[] additionalRules = null)
        {
            foreach (Vector3Int position in inTilemap.cellBounds.allPositionsWithin)
            {
                var currentTile = inTilemap.GetTile(position);

                if(currentTile != toReplace) continue;

                bool passed = true;

                for (var i = 0; additionalRules != null && i < additionalRules.Length; i++) 
                { if (!additionalRules[i].Invoke(position, currentTile, toReplace)) { passed = false; break; } }

                if(!passed) continue;

                inTilemap.SetTile(position, replacement);
            }
        }

        
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

        public static bool HasTilesAroundPoint(this Tilemap tilemap, Vector3Int position)
        {
            // Define the 8 possible neighbor offsets
            Vector3Int[] neighborOffsets = new Vector3Int[]
            {
                new Vector3Int(-1,  0, 0), // Left
                new Vector3Int( 1,  0, 0), // Right
                new Vector3Int( 0,  1, 0), // Up
                new Vector3Int( 0, -1, 0), // Down
                new Vector3Int(-1,  1, 0), // Up-Left
                new Vector3Int( 1,  1, 0), // Up-Right
                new Vector3Int(-1, -1, 0), // Down-Left
                new Vector3Int( 1, -1, 0)  // Down-Right
            };

            foreach (var offset in neighborOffsets)
            {
                Vector3Int neighborPos = position + offset;
                if (tilemap.GetTile(neighborPos) != null)
                    return true;
            }
            return false;
        }
        
        public static bool HasTilesAroundPoint<TTile>(this Tilemap tilemap, Vector3Int position)
            where TTile : TileBase
        {
            // Define the 8 possible neighbor offsets
            Vector3Int[] neighborOffsets = new Vector3Int[]
            {
                new Vector3Int(-1,  0, 0), // Left
                new Vector3Int( 1,  0, 0), // Right
                new Vector3Int( 0,  1, 0), // Up
                new Vector3Int( 0, -1, 0), // Down
                new Vector3Int(-1,  1, 0), // Up-Left
                new Vector3Int( 1,  1, 0), // Up-Right
                new Vector3Int(-1, -1, 0), // Down-Left
                new Vector3Int( 1, -1, 0)  // Down-Right
            };

            foreach (var offset in neighborOffsets)
            {
                Vector3Int neighborPos = position + offset;
                
                if (tilemap.GetTile<TTile>(neighborPos) != null)
                    return true;
            }
            return false;
        }
        
        //will expand each side on n cells
        public static void Expand(this Tilemap tilemap, int n)
        {
            tilemap.size   += new Vector3Int(n * 2,n * 2);    //adding n cells to top and to right
            tilemap.origin -= new Vector3Int(n, n);           //moving tilemap "center" on n cells down and left

            tilemap.ResizeBounds();
        }
        public static void ForEachCell(this Tilemap tilemap, Action<Vector3Int, TileBase> processTile)
        {
             for (int x = (int)tilemap.localBounds.min.x; x < tilemap.localBounds.max.x; x++)
             {
                 for (int y = (int)tilemap.localBounds.min.y; y < tilemap.localBounds.max.y; y++)
                 {
                     Vector3Int position = new Vector3Int(x, y, 0);
                     
                     processTile.Invoke(position, tilemap.GetTile(position));
                 }
             }           
        }
        public static void ForEachTile<TTile>(this Tilemap tilemap, Action<Vector3Int, TTile> processTile)
            where TTile : TileBase
        {
             for (int x = (int)tilemap.localBounds.min.x; x < tilemap.localBounds.max.x; x++)
             {
                 for (int y = (int)tilemap.localBounds.min.y; y < tilemap.localBounds.max.y; y++)
                 {
                     Vector3Int position = new Vector3Int(x, y, 0);
                     
                     if (!tilemap.HasTile<TTile>(position)) continue;

                     processTile.Invoke(position, tilemap.GetTile<TTile>(position));
                 }
             }           
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