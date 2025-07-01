using System;
using System.Collections.Generic;
using banging_code.common;
using UnityEngine;

namespace banging_code.common.rooms
{
    [Serializable]
    public sealed class ConnectSocketsHandler
    {
        [SerializeField] private List<Room.ConnectSocket> connectSockets = new();
        private List<Room.ConnectSocket> connectedSockets = new();

        public Room.ConnectSocket[] GetConnectedCorrSockets()
        {
            return connectedSockets.FindAll((socket) => socket.Purpose == Room.ConnectSocket.SocketPurpose.Corridor && socket.IsAlreadyConnected).ToArray();
        }
        
        public Room.ConnectSocket[] GetSockets()
        {
            return connectSockets.ToArray();
        }

        public Room.ConnectSocket GetCorrSocket(GameDirection direction)
        {
            foreach (var socket in GetSocketsFor(Room.ConnectSocket.SocketPurpose.Corridor))
            {   
                if (socket.Direction == direction)
                {
                    return socket;
                }
            }
            
            return null;
        }
        
        public int GetSocketsCount()
        {
            if (connectSockets == null) return 0;
            
            return connectSockets.Count;
        }

        public bool CanCorridorConnectTo(Room.ConnectSocket socket) => CanCorridorConnectTo(socket.Direction);

        public bool CanCorridorConnectTo(GameDirection direction)
            => GetSocketsFor(Room.ConnectSocket.SocketPurpose.Corridor).Find((socket) => socket.Direction == UTLS.OppositeTo(direction)) != null;

        public bool CanBudConnectFor(Room.ConnectSocket socket) 
            => GetSocketsFor(Room.ConnectSocket.SocketPurpose.Bud).Find((socket) => socket.Direction == UTLS.OppositeTo(socket.Direction)) != null;


        public void SetSockets(Room.ConnectSocket[] sockets)
        {
            connectSockets.Clear();

            foreach (var socket in sockets)
            {
                connectSockets.Add(socket);
            }
        }

        private List<Room.ConnectSocket> GetSocketsFor(Room.ConnectSocket.SocketPurpose purpose)
        {
            List<Room.ConnectSocket> sockets = new List<Room.ConnectSocket>();

            foreach (var socket in connectSockets)
            {
                if(socket.Purpose == purpose)
                    sockets.Add(socket);
            }

            return sockets;
        }
    }
}