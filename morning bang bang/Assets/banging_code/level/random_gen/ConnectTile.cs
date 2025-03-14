using banging_code.common;
using banging_code.level.rooms;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace banging_code.level.random_gen
{
    [CreateAssetMenu(menuName = "Level/Connect Tile")]
    public class ConnectTile : Tile
    {
        public GameDirection Direction;
        public Room.ConnectSocket.SocketPurpose Purpose;
    }
}