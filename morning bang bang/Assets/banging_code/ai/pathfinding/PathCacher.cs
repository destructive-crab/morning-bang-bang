using System.Collections.Generic;
using UnityEngine;

namespace banging_code.ai.pathfinding
{
    public sealed class PathCacher
    {
        private readonly Dictionary<string, Vector3[]> paths;
        private int size;

        public PathCacher(int size)
        {
            paths = new Dictionary<string, Vector3[]>(size);
            this.size = size;
        }

        public bool Add(Vector2Int start, Vector2Int end, Vector3[] path)
        {
            if (paths.Count == size)
            {
                paths.Remove(paths.GetEnumerator().Current.Key);
            }
            return paths.TryAdd(GetKeyFor(start, end), path);
        }
        
        public Vector3[] Get(Vector2Int start, Vector2Int end)
        {
            return paths.GetValueOrDefault(GetKeyFor(start, end));
        }

        private string GetKeyFor(Vector2Int start, Vector2Int end)
        {
            return start.ToString() + " " + end.ToString();
        }
    }   
}
