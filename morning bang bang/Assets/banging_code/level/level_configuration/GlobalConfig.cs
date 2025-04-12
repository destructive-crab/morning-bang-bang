using System;
using UnityEngine.Tilemaps;

namespace banging_code.common.rooms
{
    public static class GlobalConfig
    {
        public static readonly Type[] GlobalFloorTilemapComponents = { typeof(TilemapRenderer) };
        public static readonly Type[] GlobalObstaclesTilemapComponents = { typeof(TilemapRenderer), typeof(TilemapCollider2D) };
    }
}