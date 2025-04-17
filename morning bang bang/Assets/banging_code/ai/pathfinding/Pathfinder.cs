using System;
using System.Collections.Generic;
using MothDIed;
using UnityEngine;

namespace banging_code.ai.pathfinding
{
    public sealed class Pathfinder
    {
        private Vector2Int currentTarget;
        private static LevelMap LevelMap => Game.RunSystem.Data.Level.Map;

        private float DefineGCost(PathNode startPathNode, PathNode endPathNode) 
            => Vector2.Distance(new Vector2(startPathNode.X, startPathNode.Y), new Vector2(endPathNode.X, endPathNode.Y));

        private int Heuristic(PathNode first, PathNode second) 
            => Mathf.Abs(first.X - second.X) + Mathf.Abs(first.Y - second.Y);

        private bool CheckPointCollider(Vector2Int position)
        {
            var tile = LevelMap.Get(position.x, position.y);
            
            if(tile.CMNIsWalkable)
                return true;

            return false;
        }

        private List<PathNode> GetNeighbourPoints(PathNode pathNode, List<PathNode> ignoredPoints)
        {
            List<PathNode> neighbourPoints = new List<PathNode>();

            LevelMap.CellData[] pointsToCheck = LevelMap.GetConnections(pathNode.X, pathNode.Y);
            
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

        public List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            currentTarget = new Vector2Int((int)end.x, (int)end.y);
            List<PathNode> nextPoints = new List<PathNode>();

            List<PathNode> visitedPoints = new List<PathNode>();
            
            PathNode startPathNode = new PathNode((int)start.x, (int)start.y, LevelMap.Get((int)start.x, (int)start.y));
            PathNode endPathNode = new PathNode((int)end.x, (int)end.y,  LevelMap.Get((int)end.x, (int)end.y));

            PathNode currentPathNode = startPathNode;

            while (true)
            {
                if (currentPathNode.X == endPathNode.X && currentPathNode.Y == endPathNode.Y)
                    return RestorePath(currentPathNode);

                List<PathNode> neighbourPoints = GetNeighbourPoints(currentPathNode, visitedPoints);

                foreach (PathNode point in neighbourPoints)
                {
                    point.SetCosts(currentPathNode.G + DefineGCost(currentPathNode, point), Heuristic(point, endPathNode));
                    point.SetPreviousPoint(currentPathNode);
                    nextPoints.Add(point);

                    visitedPoints.Add(point);
                }
                
                nextPoints.Sort(new PointComparer());

                currentPathNode = nextPoints[0];
                nextPoints.Remove(currentPathNode);
            }
        }

        private List<Vector2> RestorePath(PathNode endPathNode)
        {
            PathNode current = endPathNode;
            List<Vector2> path = new List<Vector2>();

            do
            {
                path.Add(new Vector2(current.Cell.Center.x, current.Cell.Center.y));
                current = current.PreviousPathNode;
            } while ((current.PreviousPathNode != null));

            path.Reverse();

            return path;
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
    }
}