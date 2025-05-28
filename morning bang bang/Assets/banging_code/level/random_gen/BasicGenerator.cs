using System;
using System.Collections.Generic;
using banging_code.common;
using banging_code.common.rooms;
using banging_code.level.entity_locating;
using destructive_code.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.level.random_gen
{
    public sealed class BasicGenerator : Generator
    {
        private readonly BasicLevelConfig config;
        private readonly LevelScene level;

        private List<Room> dirtyRooms;
        private Room[] spawnedRooms;
        private BasicRoomTypes[] map;
        private TileBase backgroundTile;

        public BasicGenerator(BasicLevelConfig config, LevelScene level)
        {
            this.config = config;
            this.level = level;
        }

        public override Room[] Generate()
        {
            map = new BasicRoomTypes[config.LevelSize];
            BasicLevelMapGenerator mapGenerator = new BasicLevelMapGenerator(config);
            
            for (var i = 0; i < map.Length; i++)
            {
                map[i] = mapGenerator.GetNext(map, i, config.LevelSize);
            }

            if(GenerateDirtyRooms() != config.LevelSize)
            {
                return Regenerate();
            }
            
            DeleteUnusedCorridors();
            ConvertDirtyRoomsToLevel();
            
            //callbacks
            for (var i = 0; i < spawnedRooms.Length; i++)
            {
                spawnedRooms[i].RoomID.Get();
                spawnedRooms[i].OnGenerationFinished();
            }
            
            Debug.Log($"[BASIC GENERATOR] {spawnedRooms.Length} ROOMS WERE GENERATED; LEVEL SIZE: {config.LevelSize}");
            
            return spawnedRooms;
        }

        public override void Clear()
        {
            Debug.Log($"[BASIC LEVEL GENERATOR] {spawnedRooms?.Length} ROOMS WILL BE DELETED");
            
            spawnedRooms = null;
            dirtyRooms.Clear();
            map = null;
            
            level.Modules.Get<LevelHierarchyModule>().ClearGeneratedLevel();
        }

        public override Room[] Regenerate()
        {
            Clear();
            return Generate();
        }

        private int GenerateDirtyRooms()
        {
            if (level.Hierarchy.RoomsContainer.childCount > 0)
            {
                Debug.Log("[BASIC GENERATOR: GENERATE DIRTY ROOMS] TRYING TO GENERATE NEW LEVEL IN UNCLEARED SCENE");
                Clear();
            }
            
            List<Room> roomsWithUncheckedSockets = new List<Room>();
            dirtyRooms = new List<Room>();
            int recordedCount = 0;
            
            Room startRoom = InstantiateRoomPrefab(UTLS.RandomElement(config.StartRooms), Vector2.zero);
            RecordRoom(BasicRoomTypes.Start, startRoom);

            while (roomsWithUncheckedSockets.Count > 0 && recordedCount < config.LevelSize)
            {
                foreach (var socket in roomsWithUncheckedSockets[0].SocketsHandler.GetSockets())
                {
                    if (socket.IsAlreadyConnected) continue;

                    Room[] pool;
                    
                    switch (socket.Purpose)
                    {
                        case Room.ConnectSocket.SocketPurpose.Corridor: pool = config.Coridors; break;
                        case Room.ConnectSocket.SocketPurpose.Bud:       pool = config.GetPool(map[recordedCount]); break;
                        default: throw new ArgumentOutOfRangeException();
                    }

                    var spawnedRoom = SpawnRoom(roomsWithUncheckedSockets[0], socket, pool);

                    if (spawnedRoom == null) { continue; } // skipping this socket, in converting it will be closed
                    
                    RecordRoom(map[recordedCount], spawnedRoom);
                   
                    if (recordedCount == config.LevelSize) break; //all rooms generated, quiting loop
                }

                roomsWithUncheckedSockets.RemoveAt(0); 
            }

            return recordedCount;

            void RecordRoom(BasicRoomTypes type, Room room)
            {
                dirtyRooms.Add(room);
                room.OnSpawned();

                if (!room.IsBud || type == BasicRoomTypes.Start) 
                { roomsWithUncheckedSockets.Add(room); if(!room.IsBud) return; } 
            
                switch (type)
                {
                    case BasicRoomTypes.Start:    break;
                    case BasicRoomTypes.Gang:     recordedCount++; break;
                    case BasicRoomTypes.Buisness: recordedCount++; break;
                    case BasicRoomTypes.Treasure: recordedCount++; break; 
                    case BasicRoomTypes.Final:    recordedCount++; break;
                }                
            }
        }

        private void ConvertDirtyRoomsToLevel()
        {
            //we declare them here because we want to use them in local functions
            foreach (Room room in dirtyRooms)
            {
                if (room == null) Debug.LogError("!! NULL ROOM IN DIRTY ROOMS POOL !!");

                Grid roomGrid = room.transform.Find(G_O_NAMES.ROOM_CONTENT_ROOT).GetComponent<Grid>();
                Tilemap roomFloorTilemap = roomGrid.transform.Find(G_O_NAMES.FLOOR_TM).GetComponent<Tilemap>();
                Tilemap roomObstaclesTilemap = roomGrid.transform.Find(G_O_NAMES.OBSTACLES_TM).GetComponent<Tilemap>();
                
                foreach (Room.ConnectSocket socket in room.SocketsHandler.GetSockets())
                {
                    roomObstaclesTilemap.SetTile(socket.Tiles[0].Position, null);
                    roomObstaclesTilemap.SetTile(socket.Tiles[1].Position, null); 
                    
                    if (!socket.IsAlreadyConnected)
                    {
                        roomObstaclesTilemap.Expand(1);
                        
                        roomObstaclesTilemap.SetTile(socket.Tiles[0].Position, config.GetDefaultTileFor(socket.Direction));
                        roomObstaclesTilemap.SetTile(socket.Tiles[1].Position, config.GetDefaultTileFor(socket.Direction));
                        
                        roomObstaclesTilemap.SetTile(socket.Tiles[1].Position, config.GetDefaultTileFor(socket.Direction));
                        roomObstaclesTilemap.SetTile(socket.Tiles[1].Position, config.GetDefaultTileFor(socket.Direction));
                    }
                }
                
                roomFloorTilemap.CopyTilesTo(level.Hierarchy.FloorTilemap);
                roomObstaclesTilemap.CopyTilesTo(level.Hierarchy.ObstaclesTilemap);
               
                GameObject.Destroy(roomFloorTilemap.gameObject);
                GameObject.Destroy(roomObstaclesTilemap.gameObject);
                GameObject.Destroy(roomGrid);

                room.RoomShapeCollider.gameObject.layer = LayerMask.NameToLayer("Default");
                room.RoomShapeCollider.isTrigger = true;
                room.RoomShapeCollider.gameObject.AddComponent<RoomLocationTrigger>();
            }
            
            var globalFloorTilemap = level.Hierarchy.FloorTilemap;
            var globalObstaclesTilemap = level.Hierarchy.ObstaclesTilemap;
                
            globalFloorTilemap.ForEachCell((position, tile) =>
            {
                if(tile == null) globalFloorTilemap.SetTile(position, backgroundTile);

                if (tile is MarkTile markTile)
                {
                    globalFloorTilemap.SetTile(position, markTile.GetTile());
                }
            });
            
            globalObstaclesTilemap.ForEachTile<MarkTile>((position, tile) =>
            {
                globalObstaclesTilemap.SetTile(position, tile.GetTile());
            });
            
            globalObstaclesTilemap.Expand(5);
            globalObstaclesTilemap.ForEachCell((position, tile) =>
            {
                if(tile != null) return;
                
                if (!globalFloorTilemap.HasTile(position))
                {
                    globalObstaclesTilemap.SetTile(position, config.FillTile);
                }
            });
            
            spawnedRooms = dirtyRooms.ToArray();
            dirtyRooms.Clear();
            dirtyRooms = null;
        }
        private void DeleteUnusedCorridors()
        {
            bool emptyCycle = false;
            
            while (!emptyCycle)
            {
                emptyCycle = true;
                for (var i = 0; i < dirtyRooms.Count; i++)
                {
                    Room room = dirtyRooms[i];
                    
                    if (!room.IsBud && room.Connections.Length == 1)
                    {
                        room.DisconnectFromAll();
                        GameObject.DestroyImmediate(room.gameObject);
                        
                        dirtyRooms.RemoveAt(i);
                        i--;
                        emptyCycle = false;
                    }
                }
            }
        }

        private Room SpawnRoom(Room forRoom, Room.ConnectSocket socket, Room[] fromPool)
        {
            Vector3 spawnPosition =
                    default; //cause we use local function as re-roll predicate so it will write in this variable position for every room
            //the last room we will spawn, so this variable will contain correct spawn position for room n we don't need to invoke getPos method again

            var roomPrefab = UTLS.RandomElement(fromPool, ReRollPredicate);

            //!!!
            if (roomPrefab == null)
            {
                return null;
            }

            var newRoom = InstantiateRoomPrefab(roomPrefab, spawnPosition);

            Room.ConnectSocket newSocket = newRoom.SocketsHandler.GetCorrSocket(UTLS.OppositeTo(socket.Direction));

            UTLS.ConnectRoomsAtSockets(forRoom, newRoom, socket, newSocket);

            newRoom.ProcessContentInRoom();

            return newRoom;


            //lf r///
            bool ReRollPredicate(Room candidate)
            {
                if (candidate == null)
                {
                    Debug.LogError("NULL ROOM DETECTED IN POOL WHEN REROLLING");
                    return false;
                }

                if (candidate.SocketsHandler == null)
                {
                    Debug.LogError($"NULL SOCKET HANDLER IN {candidate.gameObject.name}");
                    return false;
                }
                
                if (!candidate.SocketsHandler.CanCorridorConnectTo(socket)) return true;

                spawnPosition = GetSpawnPositionFor(socket,
                    candidate.SocketsHandler.GetCorrSocket(UTLS.OppositeTo(socket.Direction)),
                    forRoom.transform.position);

                return !CanSpawnRoomInPosition(socket, candidate, spawnPosition, socket.Direction);
            }
        }
        
        //utility
        private bool CanSpawnRoomInPosition(Room.ConnectSocket from, Room room, Vector3 position, GameDirection direction)
        {
            switch (direction)
            {
                case GameDirection.Left:   position += new Vector3(-3, 0, 0); break;
                case GameDirection.Right:  position += new Vector3(3, 0, 0);  break;
                case GameDirection.Top:    position += new Vector3(0, 3, 0);  break;
                case GameDirection.Bottom: position += new Vector3(0, -3, 0); break;
            }
            
            Collider2D collider = GameObject
                .Instantiate(room.RoomShapeCollider.gameObject, position, Quaternion.identity)
                .GetComponent<Collider2D>();
            
            var results = new List<Collider2D>();

            var contactFilter = new ContactFilter2D
            {
                layerMask = LayerMask.GetMask("RoomBounds"),
                useLayerMask = true,
                useTriggers = false
            };

            var resCount = collider.OverlapCollider(contactFilter, results);
            
            GameObject.Destroy(collider.gameObject);
// || (resCount == 1 && results.Contains(from.Owner.RoomShapeCollider))
            return (resCount == 0);
        }

        private Vector3 GetSpawnPositionFor(Room.ConnectSocket firstSocket, Room.ConnectSocket secondSocket, Vector3 centerPosition)
        {
            Vector3 spawnPosition = centerPosition;

            spawnPosition += firstSocket.Offset;
            spawnPosition -= secondSocket.Offset;

            return spawnPosition;
        }
        
        private Room InstantiateRoomPrefab(Room prefab, Vector2 position) 
            => GameObject.Instantiate(prefab, position, Quaternion.identity, level.Hierarchy.RoomsContainer);    
    }
}