using System;
using System.Collections.Generic;
using System.Linq;
using banging_code.common;
using MothDIed;
using UnityEngine;

namespace banging_code.ai.pathfinding
{
    public sealed class Pathfinder
    {
        private readonly ID obstacleIDsToIgnore;

        public Pathfinder(ID obstacleIDsToIgnore)
        {
            this.obstacleIDsToIgnore = obstacleIDsToIgnore;
        }

        private static LevelMap LevelMap => Game.RunSystem.Data.Level.Map;

        private float DefineGCost(PathNode startPathNode, PathNode endPathNode) 
            => Vector2.Distance(new Vector2(startPathNode.X, startPathNode.Y), new Vector2(endPathNode.X, endPathNode.Y));

        private int Heuristic(PathNode first, PathNode second) 
            => Mathf.Abs(first.X - second.X) + Mathf.Abs(first.Y - second.Y);

        private bool CheckPointCollider(Vector2Int position)
        {
            var tile = LevelMap.Get(position.x, position.y);
            
            return tile.IsWalkableExcludeDynamicObstacle && (tile.Other == null || tile.Other.ID == obstacleIDsToIgnore);
        }

        private List<PathNode> GetNeighbourPoints(PathNode pathNode, List<PathNode> ignoredPoints)
        {
            List<PathNode> neighbourPoints = new List<PathNode>();
            
            LevelMap.CellData[] pointsToCheck;
            
            if (LevelMap.HasAnyAround(pathNode.Cell.Position, (cell) => !cell.IsWalkableExcludeDynamicObstacle))
            {
                pointsToCheck = LevelMap.GetConnections(pathNode.X, pathNode.Y);
            }
            else
            {
                pointsToCheck = LevelMap.GetConnectionsWithCorners(pathNode.X, pathNode.Y);
            }
            
            foreach (LevelMap.CellData nextPoint in pointsToCheck)
            {
                var node = new PathNode(nextPoint.Position.x, nextPoint.Position.y, nextPoint);
                
                if (CheckPointCollider(nextPoint.Position) && !ignoredPoints.Contains(node))
                {
                    neighbourPoints.Add(node);
                }
            }
            
            return neighbourPoints;
        }

        public Path FindPath(Vector2 start, Vector2 end)
        {
            List<PathNode> nextPoints = new List<PathNode>();

            List<PathNode> visitedPoints = new List<PathNode>();

            var startCell = LevelMap.Get(Mathf.RoundToInt(start.x), Mathf.RoundToInt(start.y));
            PathNode startPathNode = new PathNode(startCell.Position.x, startCell.Position.y, startCell);
            var endCell = LevelMap.Get(Mathf.RoundToInt(end.x), Mathf.RoundToInt(end.y));
            PathNode endPathNode = new PathNode(endCell.Position.x, endCell.Position.y, endCell);

            PathNode currentPathNode = startPathNode;
            
            do
            {
                if (currentPathNode.X == endPathNode.X && currentPathNode.Y == endPathNode.Y)
                {
                    var resPath = RestorePath(currentPathNode, end);
                    
                    return resPath;
                }

                List<PathNode> neighbourPoints = GetNeighbourPoints(currentPathNode, visitedPoints);

                foreach (PathNode point in neighbourPoints)
                {
                    point.SetCosts(currentPathNode.G + DefineGCost(currentPathNode, point), Heuristic(point, endPathNode));
                    point.SetPreviousPoint(currentPathNode);
                    nextPoints.Add(point);

                    visitedPoints.Add(point);
                }

                if (nextPoints.Count == 0)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"ENTITY IS STUCK");
#endif
                    return null;
                }
                nextPoints.Sort(new PointComparer());

                currentPathNode = nextPoints[0];
                nextPoints.Remove(currentPathNode);
            } while (nextPoints.Count > 0);

            return null;
        }

        private Path RestorePath(PathNode endPathNode, Vector2 endPosition)
        {
            PathNode current = endPathNode;
            List<Vector3> path = new List<Vector3>();

            if (endPathNode.PreviousPathNode == null) return null;
            
            do
            {
                path.Add(new Vector3(current.Cell.Center.x, current.Cell.Center.y, 0));
                current = current.PreviousPathNode;
            } while ((current.PreviousPathNode != null));

            path.Reverse();
            path.Add(endPosition);
            
            return new Path(path.ToArray());
        } 
        
        private class PathNode : IEquatable<PathNode>
        {
            public PathNode(int x, int y, LevelMap.CellData cell)
            {
                this.X = x;
                this.Y = y;
                Cell = cell;
            }
            
            public PathNode PreviousPathNode { get; private set; }
            public LevelMap.CellData Cell;

            public float F => G + H;
            
            public float G { get; private set; } = 0;
            public float H { get; private set; } = 0;
            public int X { get; private set; }
            public int Y { get; private set; }

            public void SetCosts(float g, float h)
            {
                G = g; 
                H = h;
            }
            
            public void SetPreviousPoint(PathNode startingPathNode) => PreviousPathNode = startingPathNode;
            
            public void SetXY(Vector2Int point)
            {
                X = point.x;
                Y = point.y;
            }

            public bool Equals(PathNode node)
            {
                if (node == null)
                    return false;
                
                return node.X == this.X && node.Y == this.Y;
            }
        }
        private sealed class PointComparer : IComparer<PathNode>
        {
            public int Compare(PathNode x, PathNode y)
            {
                if (x.F > y.F) return 1;
                else if (x.F < y.F) return -1;
                return 0;
            }
        }
        
        public struct PathfinderArgs
        {
            
        }

        public class Path 
        {
            public readonly Vector3[] Points;

            public Vector3 CurrentPoint => Points[CurrentIndex];
            public int CurrentIndex { get; private set; }

            public int Size => Points.Length;

            public bool Completed { get; private set; } = false;

            public Path(Vector3[] points)
            {
                Points = points;
            }

            public Vector3 Next()
            {
                CurrentIndex++;
                if (CurrentIndex >= Size)
                {
                    Completed = true;
                    return Points.Last();
                }
                
                return Points[CurrentIndex];
            }
        }
    }
}