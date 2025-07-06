using System;
using System.Collections.Generic;
using banging_code.editor.room_builder_tool;
using banging_code.level.random_gen;
using MohDIed.Tilemaps;
using UnityEngine;

namespace banging_code.common.rooms
{
    [Serializable]
    public abstract class Room : MonoBehaviour
    {
        [field: SerializeField] public Transform ContentRoot { get; private set; }
        [field: SerializeField] public ConnectSocketsHandler SocketsHandler { get; private set; } = new();
        [field: SerializeField] public PolygonCollider2D RoomShapeCollider { get; set; }
        [field: SerializeField] public bool IsBud { get; set; }

        public ID RoomID { get; private set; } = new("room", true);

        public Room[] Connections => connections.ToArray();
        private List<Room> connections = new();

        public virtual void ProcessContentInRoom()
        {
            if(ContentRoot == null)
            {
                ContentRoot = transform.Find(G_O_NAMES.ROOM_CONTENT_ROOT);
            }
        }

        public virtual void OnSpawned() { }
        
        public virtual void OnGenerationFinished() {}
        
        private void OnDestroy() => DisconnectFromAll();

        public void SetSockets(ConnectSocket[] sockets)
            => SocketsHandler.SetSockets(sockets);

        public void AddConnection(Room room) => connections.Add(room);
        public void RemoveConnection(Room room) => connections.Remove(room);

        [Serializable]
        public sealed class ConnectSocket
        {
            public bool IsAlreadyConnected
            {
                get
                {
                    if (ConnectedWith != null && ConnectedWith.Owner == null)
                        ConnectedWith = null;
                    
                    return ConnectedWith != null;
                }
            }

            public bool IsForBud => Purpose == SocketPurpose.Bud;
            public TilePT<ConnectTile> TileA => Tiles[0];
            public TilePT<ConnectTile> TileB => Tiles[1];

            public Room Owner;
            
            public Vector3 Offset;
            public GameDirection Direction;
            public SocketPurpose Purpose;
            public TilePT<ConnectTile>[] Tiles;
            
            [HideInInspector] public ConnectSocket ConnectedWith;
            
            public ConnectSocket(Tuple<TilePT<ConnectTile>, TilePT<ConnectTile>> tiles, Vector2 offset, GameDirection direction, SocketPurpose purpose, Room owner)
            {
                Tiles = new[] { tiles.Item1, tiles.Item2 };
                
                Offset = offset;
                Direction = direction;
                Purpose = purpose;
                Owner = owner;
            }
            public ConnectSocket(TilePT<ConnectTile> tile1, TilePT<ConnectTile> tile2, Vector2 offset, GameDirection direction, SocketPurpose purpose, Room owner)
            {
                Tiles = new[] { tile1, tile2 };
                
                Offset = offset;
                Direction = direction;
                Purpose = purpose;
                Owner = owner;
            }
            
            public enum SocketPurpose
            {
                Corridor,
                Bud,
            }
        }


#if UNITY_EDITOR
private void OnDrawGizmos() => SocketDrawer.DrawSocketsFor(this);
#endif

        public void DisconnectFromAll()
        {
            while(connections.Count > 0)
            {
               connections[0].RemoveConnection(this); 
               connections.RemoveAt(0);
            }

            foreach (var socket in SocketsHandler.GetSockets())
            {
                if(socket.IsAlreadyConnected)
                {
                    socket.ConnectedWith.ConnectedWith = null;
                    socket.ConnectedWith = null;
                }
            }
        }
    }
}  