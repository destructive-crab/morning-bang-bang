using System;
using System.Collections.Generic;
using banging_code.level.structure.map;
using MothDIed.Scenes.SceneModules;
using UnityEngine;

namespace banging_code.ai.pathfinding
{
    public  abstract class LevelMap : SceneModule
    {
        protected readonly Dictionary<Vector2Int, CellData> map = new();
        protected readonly List<DynamicObstacle> dynamicObstacles = new();

        public void NewDynamicObstacle(DynamicObstacle dynamicObstacle)
        {
            dynamicObstacles.Add(dynamicObstacle);
        }        
        public void ClearDyObListFromNulls()
        {
            for(int i = 0; i < dynamicObstacles.Count; i++)
            {
                if(dynamicObstacles[i] == null)
                {
                    dynamicObstacles.RemoveAt(i);
                    i--;
                }
            }
        }
        
        public CellData Get(Vector2Int position)
        {
            return map[position];
        }

        public CellData Get(int positionX, int positionY)
        {
            return Get(new Vector2Int(positionX, positionY));
        }

        public abstract void UpdateCell(Vector2Int position);
        public abstract void Refresh(); //update all cells

        public abstract bool HasAnyAround(Vector2Int position, Predicate<CellData> check);

        private CellData[] GetConnections(int x, int y, Vector2Int[] offsets)
        {
            Vector2Int position = new(x, y);

            List<CellData> res = new();

            foreach (var offset in offsets)
            {
                if (map.TryGetValue(position + offset, out CellData cellData))
                {
                    res.Add(cellData);
                }
            }

            return res.ToArray();
        }
        
        public CellData[] GetConnectionsWithCorners(int x, int y)
        {
            Vector2Int[] offsets = new Vector2Int[]
            {
                new Vector2Int(-1,  0), // Left
                new Vector2Int( 1,  0), // Right
                new Vector2Int( 0,  1), // Up
                new Vector2Int( 0, -1), // Down
                new Vector2Int(-1,  1), // Up-Left
                new Vector2Int( 1,  1), // Up-Right
                new Vector2Int(-1, -1), // Down-Left
                new Vector2Int( 1, -1)  // Down-Right
            };
            
            return GetConnections(x, y, offsets);
        }

        public CellData[] GetConnections(int x, int y)
        {
            Vector2Int[] offsets = new Vector2Int[]
            {
                new Vector2Int(-1,  0), // Left
                new Vector2Int( 1,  0), // Right
                new Vector2Int( 0,  1), // Up
                new Vector2Int( 0, -1), // Down
            };
            return GetConnections(x, y, offsets);
        }

        public class CellData
        {
            public bool CMNIsWalkable => Obstacle == null && Floor != null && Floor.IsWalkable() && Other == null;

            public readonly Vector2Int Position;
            public Vector3 Center;
            public CellData[] Connections;
            
            public IFloorTile Floor;
            public IObstacleTile Obstacle;

            public DynamicObstacle Other;

            public CellData(Vector2Int position)
            {
                Position = position;
            }
        }
    }
}