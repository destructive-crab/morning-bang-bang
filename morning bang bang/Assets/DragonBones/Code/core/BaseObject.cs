using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The BaseObject is the base class for all objects in the DragonBones framework.
    /// All BaseObject instances are cached to the object pool to reduce the performance consumption of frequent requests for memory or memory recovery.
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>
    public abstract class BaseObject
    {
        private static uint _hashCode = 0;
        private static uint _defaultMaxCount = 3000;
        private static readonly Dictionary<System.Type, uint> _maxCountMap = new Dictionary<System.Type, uint>();
        private static readonly Dictionary<System.Type, List<BaseObject>> _poolsMap = new Dictionary<System.Type, List<BaseObject>>();

        private static void _ReturnObject(BaseObject obj)
        {
            var classType = obj.GetType();
            var maxCount = _maxCountMap.ContainsKey(classType) ? _maxCountMap[classType] : _defaultMaxCount;
            var pool = _poolsMap.ContainsKey(classType) ? _poolsMap[classType] : _poolsMap[classType] = new List<BaseObject>();

            if (pool.Count < maxCount)
            {
                if (!pool.Contains(obj))
                {
                    pool.Add(obj);
                }
                else
                {
                    Helper.Assert(false, "The object is already in the pool.");
                }
            }
            else
            {
                //TODO: addwarning
            }
        }

        /// <summary>
        /// - Set the maximum cache count of the specify object pool.
        /// </summary>
        /// <param name="objectConstructor">- The specify class. (Set all object pools max cache count if not set)</param>
        /// <param name="maxCount">- Max count.</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public static void SetMaxCount(System.Type classType, uint maxCount)
        {
            if (classType != null)
            {
                if (_poolsMap.ContainsKey(classType))
                {
                    var pool = _poolsMap[classType];
                    if (pool.Count > maxCount)
                    {
                        pool.ResizeList((int)maxCount, null);
                    }
                }

                _maxCountMap[classType] = maxCount;
            }
            else
            {
                _defaultMaxCount = maxCount;

                foreach (var key in _poolsMap.Keys)
                {
                    var pool = _poolsMap[key];
                    if (pool.Count > maxCount)
                    {
                        pool.ResizeList((int)maxCount, null);
                    }

                    if (_maxCountMap.ContainsKey(key))
                    {
                        _maxCountMap[key] = maxCount;
                    }
                }
            }
        }

        /// <summary>
        /// - Clear the cached instances of a specify object pool.
        /// </summary>
        /// <param name="objectConstructor">- Specify class. (Clear all cached instances if not set)</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public static void ClearPool(System.Type classType)
        {
            if (classType != null)
            {
                if (_poolsMap.ContainsKey(classType))
                {
                    var pool = _poolsMap[classType];
                    if (pool != null)
                    {
                        pool.Clear();
                    }
                }
            }
            else
            {
                foreach (var pair in _poolsMap)
                {
                    var pool = _poolsMap[pair.Key];
                    if (pool != null)
                    {
                        pool.Clear();
                    }
                }
            }
        }
        /// <summary>
        /// - Get an instance of the specify class from object pool.
        /// </summary>
        /// <param name="objectConstructor">- The specify class.</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public static T BorrowObject<T>() where T : BaseObject, new()
        {
            var type = typeof(T);
            var pool = _poolsMap.ContainsKey(type) ? _poolsMap[type] : null;
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
                obj._OnClear();
                return obj;
            }
        }
        /// <summary>
        /// - A unique identification number assigned to the object.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public readonly uint hashCode = _hashCode++;

        protected BaseObject()
        {
        }

        /// <private/>
        protected abstract void _OnClear();
        /// <summary>
        /// - Clear the object and return it back to object pool。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        public void ReturnToPool()
        {
            _OnClear();
            _ReturnObject(this);
        }

        // public static implicit operator bool(BaseObject exists)
        // {
        //     return exists != null;
        // }
    }
}