using System;
using banging_code.common;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.common.rooms
{
    [Serializable]
    public sealed class BasicLevelConfig
    {
        public int LevelSize; 
        public int TreasuresLimit;
        public int BuisnessLimit;
        
        public Room[] StartRooms;
        public Room[] Coridors;
        public Room[] GangRooms;
        public Room[] TreasureRooms;
        public Room[] BuisnessRooms;
        public Room[] FinalRooms;

        [Header("Default tiles")]
        public TileBase[] TopDefaultWall;
        public TileBase[] BottomDefaultWall;
        public TileBase[] LeftDefaultWall;
        public TileBase[] RightDefaultWall;

        public TileBase GetDefaultTileFor(GameDirection direction)
        {
            switch (direction)
            {
                case GameDirection.Left:   return UTLS.RandomElement(LeftDefaultWall);
                case GameDirection.Right:  return UTLS.RandomElement(RightDefaultWall);
                case GameDirection.Top:    return UTLS.RandomElement(TopDefaultWall);
                case GameDirection.Bottom: return UTLS.RandomElement(BottomDefaultWall);
            }
            
            return null;
        }

        public Room[] GetPool(BasicRoomTypes type)
        {
            switch (type)
            {
                case BasicRoomTypes.Start:
                    return StartRooms;
                case BasicRoomTypes.Gang:
                    return GangRooms;
                case BasicRoomTypes.Buisness:
                    return BuisnessRooms;
                case BasicRoomTypes.Treasure:
                    return TreasureRooms;
                case BasicRoomTypes.Final:
                    return FinalRooms;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
    }
}