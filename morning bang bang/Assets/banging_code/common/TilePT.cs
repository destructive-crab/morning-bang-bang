using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.common
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