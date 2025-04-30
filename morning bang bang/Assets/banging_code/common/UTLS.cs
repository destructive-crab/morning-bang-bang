using System;
using banging_code.common.rooms;
using UnityEngine;

using Random = UnityEngine.Random;

namespace banging_code.common
{
    public static class UTLS //utilities 
    {
        //SUPER COMMON
        public static T RandomElement<T>(T[] array, int from = 0, int to = 0)
        {
            if (to == 0)
            {
                to = array.Length;
            }
            
            return array[Random.Range(from, to)];
        }
        
        public static T RandomElement<T>(T[] array, Predicate<T> reroll,int from = 0, int to = 0)
            where T : class
        {
            if (to == 0)
            {
                to = array.Length;
            }

            var result = array[Random.Range(from, to)];
            
            for(int i = 0; reroll.Invoke(result); i++)
            {
                if (i > array.Length * 2)
                {
                    return null;
                }
                
                result = array[Random.Range(from, to)];
            }
            
            return result;
        }

        //SUPER UNCOMMON
        public static Vector2 DirectionToVector(GameDirection direction)
        {
            switch (direction)
            {
                case GameDirection.Left: return Vector2.left;
                case GameDirection.Right: return Vector2.right;
                case GameDirection.Top: return Vector2.up;
                case GameDirection.Bottom: return Vector2.down;
            }

            return Vector2.zero;
        }
        public static GameDirection OppositeTo(GameDirection direction)
        {
            switch (direction)
            {
                case GameDirection.Left:   return GameDirection.Right;
                case GameDirection.Right:  return GameDirection.Left;
                case GameDirection.Top:    return GameDirection.Bottom;
                case GameDirection.Bottom: return GameDirection.Top;
            }

            throw new Exception("WHAT ARE U DOING MAAAN");
        }

        public static void ConnectRoomsAtSockets(Room firstRoom, Room secondRoom, Room.ConnectSocket firstSocket, Room.ConnectSocket secondSocket)
        {
            firstRoom.AddConnection(secondRoom);
            secondRoom.AddConnection(firstRoom);
            
            firstSocket.ConnectedWith = secondSocket;
            secondSocket.ConnectedWith = firstSocket;
        }
        public static Vector3[] ConvertV2ToV3(Vector2[] input)
        {
            Vector3[] output = new Vector3[input.Length];
            for(int i = 0; i < input.Length; i++)
            {
                output[i] = input[i];
            }
            return output;
        }
    }
}