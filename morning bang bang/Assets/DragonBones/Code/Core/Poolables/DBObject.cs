using System.Collections.Generic;
using MothDIed.Pool;

namespace DragonBones
{
    /// <summary>
    /// - The DBObject is the base class for all objects in the DragonBones framework.
    /// All DBObject instances are cached to the object pool to reduce the performance consumption of frequent requests for memory or memory recovery.
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>
    public abstract class DBObject : IPoolable
    {
        /// <summary>
        /// - A unique identification number assigned to the object.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public readonly uint HashCode = lastHashCode++;
        
        private static uint lastHashCode = 0;
        private static uint defaultPoolLimit = 3000;
        
        private static readonly Dictionary<System.Type, uint> limitMap = new();
        private static readonly Dictionary<System.Type, List<DBObject>> poolsMap = new();

        /// <summary>
        /// - Set the maximum cache count of the specify object pool.
        /// </summary>
        /// <param name="classType">- The specify class. (Set all object pools max cache count if not set)</param>
        /// <param name="limit">- Max count.</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public static void SetLimitFor(System.Type classType, uint limit)
        {
            if (classType != null)
            {
                if (poolsMap.TryGetValue(classType, out var pool))
                {
                    if (pool.Count > limit)
                    {
                        pool.ResizeList((int)limit, null);
                    }
                }

                limitMap[classType] = limit;
            }
            else
            {
                defaultPoolLimit = limit;

                foreach (var key in poolsMap.Keys)
                {
                    var pool = poolsMap[key];
                    if (pool.Count > limit)
                    {
                        pool.ResizeList((int)limit, null);
                    }

                    if (limitMap.ContainsKey(key))
                    {
                        limitMap[key] = limit;
                    }
                }
            }
        }

        /// <summary>
        /// - Clear the cached instances of a specify object pool.
        /// </summary>
        /// <param name="classType">- Specify class. (Clear all cached instances if not set)</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public static void ClearPool(System.Type classType)
        {
            if (classType != null)
            {
                if (poolsMap.TryGetValue(classType, out var pool))
                {
                    pool?.Clear();
                }
            }
            else
            {
                foreach (var pair in poolsMap)
                {
                    var pool = poolsMap[pair.Key];
                    
                    pool?.Clear();
                }
            }
        }

        /// <summary>
        /// - Get an instance of the specify class from object pool.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public static T BorrowObject<T>() where T : DBObject, new()
        {
            var type = typeof(T);
            var pool = poolsMap.ContainsKey(type) ? poolsMap[type] : null;
            if (pool != null && pool.Count > 0)
            {
                var index = pool.Count - 1;
                var obj = pool[index];
                pool.RemoveAt(index);
                return (T)obj;
            }
            else
            {
                var obj = new T();
                obj.OnReleased();
                return obj;
            }
        }

        private static void ReturnObject(DBObject obj)
        {
            var classType = obj.GetType();
            var maxCount = limitMap.ContainsKey(classType) ? limitMap[classType] : defaultPoolLimit;
            var pool = poolsMap.ContainsKey(classType) ? poolsMap[classType] : poolsMap[classType] = new List<DBObject>();

            if (pool.Count < maxCount)
            {
                if (!pool.Contains(obj))
                {
                    pool.Add(obj);
                }
                else
                {
                    DBLogger.Assert(false, "The object is already in the pool.");
                }
            }
            else
            {
                DBLogger.Warn("Pool is full");
            }
        }

        /// <summary>
        /// - Clear the object and return it back to object pool。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public void ReleaseThis()
        {
            OnReleased();
            ReturnObject(this);
        }

        public virtual void OnPicked() { }
        public abstract void OnReleased();
        
    }
}