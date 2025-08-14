using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DragonBones
{
    public class DBFrameCacher
    {
        public int CacheFrameRate { get; private set; }

        public DBFrameCacher(int cacheFrameRate)
        {
            CacheFrameRate = cacheFrameRate;
        }

        /// <summary>
        /// - All previous cache will be deleted
        /// </summary>
        public void SetCacheFrameRate(int newCacheFrameRate)
        {
            Clear();
            CacheFrameRate = newCacheFrameRate;
        }

        /// <summary>
        /// string key - armaturename_animationname_objectname(bone or slot name)
        /// float list value -
        /// frameIndex+0 - DBMatrix.a                     frameIndex+1 - DBMatrix.b
        /// frameIndex+2 - DBMatrix.c                     frameIndex+3 - DBMatrix.d
        /// frameIndex+4 - DBMatrix.tx                    frameIndex+5 - DBMatrix.ty
        /// frameIndex+6 - DBTransform.rotation     frameIndex+7 - DBTransform.skew
        /// frameIndex+8 - DBTransform.scaleX       frameIndex+9 - DBTransform.scaleY
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, float[]>> cachedFrames = new();
        /// <summary>
        /// key - armaturename_animationname
        /// </summary>
        private readonly Dictionary<string, bool[]> isCachedMarkers = new();
        
        public void Clear()
        {
            cachedFrames.Clear();
        }

        public void ClearAllOfArmature(string armatureName)
        {
            string[] keys = cachedFrames.Keys.ToArray();

            for (var i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                
                if (key.StartsWith(armatureName))
                {
                    cachedFrames.Remove(key);
                }
            }
        }
        public void ClearAnimation(string animationName)
        {
            string[] keys = cachedFrames.Keys.ToArray();

            for (var i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                
                if (key.EndsWith(animationName))
                {
                    cachedFrames.Remove(key);
                }
            }
        }
        
        public void SetCacheFrame(AnimationData animation, string objectName, int frameIndex, DBMatrix globalTransformDBMatrix, DBTransform transform)
        {
            float[] dataArray = GetOrAddCacheEntryOf(animation, objectName);

            dataArray[frameIndex] = globalTransformDBMatrix.a;
            dataArray[frameIndex + 1] = globalTransformDBMatrix.b;
            dataArray[frameIndex + 2] = globalTransformDBMatrix.c;
            dataArray[frameIndex + 3] = globalTransformDBMatrix.d;
            
            dataArray[frameIndex + 4] = globalTransformDBMatrix.tx;
            dataArray[frameIndex + 5] = globalTransformDBMatrix.ty;
            
            dataArray[frameIndex + 6] = transform.rotation;
            dataArray[frameIndex + 7] = transform.skew;
            
            dataArray[frameIndex + 8] = transform.scaleX;
            dataArray[frameIndex + 9] = transform.scaleY;

            MarkAsCached(animation, frameIndex);
        }

        public void GetCacheFrame(AnimationData data, string objectName, int frameIndex, DBMatrix globalTransformDBMatrix, DBTransform transform)
        {
            float[] dataArray = GetOrAddCacheEntryOf(data, objectName);
            
            globalTransformDBMatrix.a = dataArray[frameIndex];
            globalTransformDBMatrix.b = dataArray[frameIndex + 1];
            globalTransformDBMatrix.c = dataArray[frameIndex + 2];
            globalTransformDBMatrix.d = dataArray[frameIndex + 3];
            globalTransformDBMatrix.tx = dataArray[frameIndex + 4];
            globalTransformDBMatrix.ty = dataArray[frameIndex + 5];
            
            transform.rotation = dataArray[frameIndex + 6];
            transform.skew = dataArray[frameIndex + 7];
            transform.scaleX = dataArray[frameIndex + 8];
            transform.scaleY = dataArray[frameIndex + 9];
            
            transform.x = globalTransformDBMatrix.tx;
            transform.y = globalTransformDBMatrix.ty;
        }

        private void MarkAsCached(AnimationData animation, int frameIndex)
        {
            string animationKey = GetAnimationKey(animation);
            isCachedMarkers.TryAdd(animationKey, new bool[GetCacheFramesCountOf(animation)]);
            
            if (frameIndex >= isCachedMarkers[animationKey].Length)
            {
                DBLogger.LogWarning("Frame index to cache is above frames count");
            }
            
            isCachedMarkers[animationKey][frameIndex] = true;
        }

        public bool IsFrameCached(AnimationData animation, int frameIndex)
        {
            return isCachedMarkers.ContainsKey(GetAnimationKey(animation)) && isCachedMarkers[GetAnimationKey(animation)][frameIndex];
        }
        
        private static string GetAnimationKey(AnimationData animation)
        {
            return animation.armatureData.name + "_" + animation.name;
        }

        private string GetCacheKeyOf(AnimationData data,  string objectName) => data.armatureData.name+"_"+data.name+"_"+objectName;
        private float[] GetOrAddCacheEntryOf(AnimationData animation, string objectName)
        {
            string key = GetCacheKeyOf(animation, objectName);
            if (cachedFrames.TryAdd(key, new Dictionary<string, float[]>()))
            {
                cachedFrames[key].Add(animation.name, new float[GetCacheFramesCountOf(animation)*10]);
            }
            return cachedFrames[key][animation.name];
        }

        public int GetCacheFramesCountOf(AnimationData animation) => Mathf.CeilToInt(CacheFrameRate * animation.duration);
    }
}