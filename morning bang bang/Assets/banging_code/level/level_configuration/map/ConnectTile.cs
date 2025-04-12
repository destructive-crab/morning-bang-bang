using banging_code.common;
using banging_code.common.rooms;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.level.random_gen
{
    [CreateAssetMenu(menuName = "Tiles/Socket Mark")]
    public class ConnectTile : Tile
    {
        public GameDirection Direction;
        public Room.ConnectSocket.SocketPurpose Purpose;
    }
}