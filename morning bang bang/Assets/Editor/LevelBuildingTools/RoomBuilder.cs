using System;
using System.Collections.Generic;
using banging_code.common;
using banging_code.level.random_gen;
using banging_code.level.rooms;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace banging_code.editor.room_builder_tool
{
    public class RoomBuilder : EditorWindow
    {
        private static bool socketDrawerEnabled = true;
        
        private static Room editingRoom;
        
        private static Tilemap obstaclesTilemap;
        private static Tilemap floorTilemap;
        
        [MenuItem("Tools/Room Builder")]
        private static void ShowWindow()
        {
            GetWindow(typeof(RoomBuilder));
        }

        private void OnSelectionChange()
        {
            if (Selection.activeTransform != null && Selection.activeTransform.root.TryGetComponent<Room>(out Room room))
            {
                editingRoom = room;
                
                ReloadTilemaps();
            }
            else
            {
                editingRoom = null;
            }
        }
        private void OnGUI()
        {
            if (!CheckSelectedRoom()) return;

            SocketDrawer.Enabled = SocketDrawerToggle();
            
            ShowAdditionalInfo();

            GUILayout.Space(15);
            
            //buttons layer
            if (GUILayout.Button("Update all room's stuff"))
            {
                CreateRoomSockets();
                editingRoom.RoomShapeCollider = CreateRoomShapeCollider();
            }
            
            GUILayout.Space(15);
            
            if (GUILayout.Button("Create Sockets"))
            {
                CreateRoomSockets();
            }
            if (GUILayout.Button("Create Room Shape Collider"))
            {
                editingRoom.RoomShapeCollider = CreateRoomShapeCollider();
            }
            
            GUILayout.Space(15);
            
            if (GUILayout.Button("Create empty content room pattern"))
            {
                CreateEmptyRoomBasePattern();
            }
            
            //finish iteration
            if (GUI.changed)
            {
                EditorUtility.SetDirty(editingRoom);
            }
        }

        private bool SocketDrawerToggle()
        {
            socketDrawerEnabled = GUILayout.Toggle(socketDrawerEnabled, "Enable Socket Drawer");
            GUILayout.Space(10);
            return socketDrawerEnabled;
        }

        private static void CreateEmptyRoomBasePattern()
        {
            var newContentRoot = new GameObject(G_O_NAMES.ROOM_CONTENT_ROOT).AddComponent<Grid>();
            newContentRoot.transform.parent = editingRoom.transform;
                
            var newFloorTilemap = new GameObject(G_O_NAMES.FLOOR_TM).AddComponent<TilemapRenderer>().GetComponent<Tilemap>();
            newFloorTilemap.transform.parent = newContentRoot.transform;
                
            var newObstaclesTilemap = new GameObject(G_O_NAMES.OBSTACLES_TM)
                .AddComponent<TilemapRenderer>()
                .AddComponent<TilemapCollider2D>()
                .GetComponent<Tilemap>();
                
            newObstaclesTilemap.transform.parent = newContentRoot.transform;
            
            ReloadTilemaps();
        }

        private static void CreateRoomSockets()
        { 
            List<TilePT<ConnectTile>> tiles = obstaclesTilemap.FindTilesOfType<ConnectTile>();
                    
            var horizontalPairs = new List<Tuple<TilePT<ConnectTile>, TilePT<ConnectTile>>>();
            var verticalPairs = new List<Tuple<TilePT<ConnectTile>, TilePT<ConnectTile>>>();

            //searching for all pairs
            while(tiles.Count > 0)
            {
                var firstTile = tiles[0];
                
                for (var secondIndex = 1; secondIndex < tiles.Count; secondIndex++)
                {
                    var secondTile = tiles[secondIndex];
                    
                    //there cant be that one row/column contains multiple corridor slots 
                    
                    //checking if ycord is similar
                    if (firstTile.Position.y == secondTile.Position.y && Mathf.Abs(firstTile.Position.x - secondTile.Position.x) == 1) 
                    {
                        horizontalPairs.Add(new Tuple<TilePT<ConnectTile>, TilePT<ConnectTile>>(firstTile, secondTile)); 
                        
                        tiles.Remove(firstTile);
                        tiles.Remove(secondTile);
                        
                        break;
                    }
                    
                    //checking if xcord is similar
                    if (firstTile.Position.x == secondTile.Position.x && Mathf.Abs(firstTile.Position.y - secondTile.Position.y) == 1)
                    {
                        verticalPairs.Add(new Tuple<TilePT<ConnectTile>, TilePT<ConnectTile>>(firstTile, secondTile));
                        
                        tiles.Remove(firstTile);
                        tiles.Remove(secondTile);
                        
                        break;
                    }
                }
            }
            
            //cooking sockets
            List<Room.ConnectSocket> connectSockets = new List<Room.ConnectSocket>();
            
            foreach (var pair in horizontalPairs)
            {
                float y = obstaclesTilemap.GetCellCenterWorld(new Vector3Int(0, pair.Item1.Position.y)).y;
                
                if (!floorTilemap.HasTile(pair.Item1.Position + Vector3Int.up))
                {
                    pair.Item1.Tile.Direction = GameDirection.Top;
                    pair.Item2.Tile.Direction = GameDirection.Top;

                    y += 0.5f;
                }
                else if (!floorTilemap.HasTile(pair.Item1.Position + Vector3Int.down))
                {
                    pair.Item1.Tile.Direction = GameDirection.Bottom;
                    pair.Item2.Tile.Direction = GameDirection.Bottom;
                    
                    y -= 0.5f;
                }
                
                int fromX = pair.Item1.Position.x;
                int toX = pair.Item2.Position.x;

                float centerX = (obstaclesTilemap.GetCellCenterWorld(new Vector3Int(fromX, 0)).x 
                                 + obstaclesTilemap.GetCellCenterWorld(new Vector3Int(toX, 0)).x) 
                                / 2;
                
                connectSockets.Add(new Room.ConnectSocket(pair,
                    new Vector2(centerX, y), pair.Item1.Tile.Direction, pair.Item1.Tile.Purpose, editingRoom));
            }
            
            foreach (var pair in verticalPairs)
            {
                float x = obstaclesTilemap.GetCellCenterWorld(new Vector3Int(pair.Item1.Position.x, 0)).x;
                
                if (!floorTilemap.HasTile(pair.Item1.Position + Vector3Int.right))
                {
                    pair.Item1.Tile.Direction = GameDirection.Right;
                    pair.Item2.Tile.Direction = GameDirection.Right;

                    x += 0.5f;
                }
                else if (!floorTilemap.HasTile(pair.Item1.Position + Vector3Int.left))
                {
                    pair.Item1.Tile.Direction = GameDirection.Left;
                    pair.Item2.Tile.Direction = GameDirection.Left;
                    
                    x -= 0.5f;
                }
                
                int fromY = pair.Item1.Position.y;
                int toY = pair.Item2.Position.y;

                float centerY = (obstaclesTilemap.GetCellCenterWorld(new Vector3Int(0, fromY)).y 
                                 + obstaclesTilemap.GetCellCenterWorld(new Vector3Int(0, toY)).y) 
                                / 2;
                
                connectSockets.Add(new Room.ConnectSocket(
                    pair, 
                    new Vector2(x, centerY),
                    pair.Item1.Tile.Direction,
                    pair.Item1.Tile.Purpose,
                    editingRoom));
            }

            editingRoom.SetSockets(connectSockets.ToArray());
        }

        private static void ShowAdditionalInfo()
        {
            EditorGUI.BeginDisabledGroup(true);
                
            EditorGUILayout.ObjectField("Selected: ", editingRoom, typeof(Room), false);
            EditorGUILayout.ObjectField("Floor: ", floorTilemap, typeof(Tilemap), false);
            EditorGUILayout.ObjectField("Obstacles: ", obstaclesTilemap, typeof(Tilemap), false);
                
            EditorGUI.EndDisabledGroup();
        }

        private static PolygonCollider2D CreateRoomShapeCollider()
        {
            TilemapCollider2D _tilemapCollider;
            Mesh _roomMesh;
            PolygonCollider2D polyCollider_roomBoundsCollider = GetFromRoom<PolygonCollider2D>(G_O_NAMES.ROOM_SHAPE_COLLIDER);
            
            //creating new polygon collider
            if(polyCollider_roomBoundsCollider == null)
            { 
                polyCollider_roomBoundsCollider = new GameObject(G_O_NAMES.ROOM_SHAPE_COLLIDER).AddComponent<PolygonCollider2D>();
            
                polyCollider_roomBoundsCollider.transform.parent = editingRoom.transform;
                polyCollider_roomBoundsCollider.gameObject.layer = LayerMask.NameToLayer("RoomBounds");
            
                Undo.RecordObject(polyCollider_roomBoundsCollider.gameObject, "Room Bounds Collider Creation");
            }
            polyCollider_roomBoundsCollider.pathCount = 0;
            
            //creating tilemap collider
            {
                var collisionTile = AssetDatabase.LoadAssetAtPath<SquareCollisionTile>("Assets/Editor/LevelBuildingTools/CollisionTile.asset");
                var grid = editingRoom.transform.Find(G_O_NAMES.ROOM_CONTENT_ROOT).GetComponentInChildren<Grid>();

                var _tempTilemap = new GameObject("TEMP TM COLLIDER").AddComponent<Tilemap>();
                _tilemapCollider = _tempTilemap.gameObject.AddComponent<TilemapCollider2D>();
                _tempTilemap.gameObject.AddComponent<TilemapRenderer>();
                _tempTilemap.transform.parent = grid.transform;
                _tempTilemap.transform.localPosition = Vector2.zero;

                foreach (var position in floorTilemap.cellBounds.allPositionsWithin)
                {
                    if (floorTilemap.HasTile(position))
                    {
                        _tempTilemap.SetTile(position, collisionTile);
                    }
                }

                foreach (var position in obstaclesTilemap.cellBounds.allPositionsWithin)
                {
                    if (obstaclesTilemap.HasTile(position))
                    {
                        _tempTilemap.SetTile(position, collisionTile);
                    }
                }
            }
            
            //we have room mesh, so we can destroy tm collider 
            {
                _tilemapCollider.useDelaunayMesh = true;
                _roomMesh = _tilemapCollider.CreateMesh(true, true);

                DestroyImmediate(_tilemapCollider.gameObject);
            }
            
            //some polygon generation code from WWW (idont understand it): 
            int[] triangles = _roomMesh.triangles;
            Vector3[] vertices = _roomMesh.vertices;

            // Get just the outer edges from the mesh's triangles (ignore or remove any shared edges)
            Dictionary<string, KeyValuePair<int, int>> edges = new Dictionary<string, KeyValuePair<int, int>>();
            
            for (int i = 0; i < triangles.Length; i += 3) 
            {
                for (int e = 0; e < 3; e++) 
                {
                    int vert1 = triangles[i + e];
                    int vert2 = triangles[i + e + 1 > i + 2 ? i : i + e + 1];
                    string edge = Mathf.Min(vert1, vert2) + ":" + Mathf.Max(vert1, vert2);
                    
                    if (edges.ContainsKey(edge)) 
                    {
                        edges.Remove(edge);
                    }
                    else 
                    {
                        edges.Add(edge, new KeyValuePair<int, int>(vert1, vert2));
                    }
                }
            }

            // Create edge lookup (Key is first vertex, Value is second vertex, of each edge)
            Dictionary<int, int> lookup = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> edge in edges.Values) 
            {
                if (lookup.ContainsKey(edge.Key) == false) 
                {
                    lookup.Add(edge.Key, edge.Value);
                }
            }
            // Loop through edge vertices in order
            int startVert = 0;
            int nextVert = startVert;
            int highestVert = startVert;
            List<Vector2> colliderPath = new List<Vector2>();
            
            while (true) {

                // Add vertex to collider path
                colliderPath.Add(vertices[nextVert]);

                // Get next vertex
                nextVert = lookup[nextVert];

                // Store highest vertex (to know what shape to move to next)
                if (nextVert > highestVert) {
                    highestVert = nextVert;
                }

                // Shape complete
                if (nextVert == startVert) {

                    // Add path to polygon collider
                    polyCollider_roomBoundsCollider.pathCount++;
                    polyCollider_roomBoundsCollider.SetPath(polyCollider_roomBoundsCollider.pathCount - 1, colliderPath.ToArray());
                    colliderPath.Clear();

                    // Go to next shape if one exists
                    if (lookup.ContainsKey(highestVert + 1)) {

                        // Set starting and next vertices
                        startVert = highestVert + 1;
                        nextVert = startVert;

                        continue;
                    }

                    break;
                }
            }
            
            return polyCollider_roomBoundsCollider;
        }
        private static Room GetSelectedRoom()
        {
            if (Selection.activeGameObject != null)
            {
                Selection.activeGameObject.TryGetComponent(out Room room);
                return room; 
            }

            return null;
        }

        private static T GetFromContentRoot<T>(string path)
            where T : Component
        {
            var target = editingRoom.transform.Find(G_O_NAMES.ROOM_CONTENT_ROOT).Find(path);

            if (target == null)
                return null;
            
            return target.GetComponent<T>();
        }

        private static T GetFromRoom<T>(string path)
            where T : Component
        {
            var target = editingRoom.transform.Find(path);

            if (target == null)
                return null;
            
            return target.GetComponent<T>();
        }

        private static bool CheckSelectedRoom() => editingRoom != null;

        private static void ReloadTilemaps()
        {
            if(editingRoom == null) return;
            
            obstaclesTilemap = GetFromContentRoot<Tilemap>(G_O_NAMES.OBSTACLES_TM);
            floorTilemap = GetFromContentRoot<Tilemap>(G_O_NAMES.FLOOR_TM);
        }
    }
}