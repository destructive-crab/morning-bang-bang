using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace destructive_code.Tilemaps
{
    [Serializable]
    public sealed class TilePT<TileT>
        where TileT : Tile
    {
        public TilePT(Vector3Int position, TileT tile)
        {
            Position = position;
            Tile = tile;
        }

        public Vector3Int Position;
        public TileT Tile;
    }
}