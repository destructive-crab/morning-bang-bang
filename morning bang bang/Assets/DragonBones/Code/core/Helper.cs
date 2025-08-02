using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DragonBones
{
    internal static class Helper
    {
        public static readonly int INT16_SIZE = 2;
        public static readonly int UINT16_SIZE = 2;
        public static readonly int FLOAT_SIZE = 4;

        internal static void Assert(bool condition, string message)
        {
            Debug.Assert(condition, message);
        }

        internal static void ResizeList<T>(this List<T> list, int count, T value = default(T))
        {
            if (list.Count == count)
            {
                return;
            }

            if (list.Count > count)
            {
                list.RemoveRange(count, list.Count - count);
            }
            else
            {
                //fixed gc,may be memory will grow
                //list.Capacity = count;
                for (int i = list.Count, l = count; i < l; ++i)
                {
                    list.Add(value);
                }
            }
        }

        internal static List<float> Convert(this List<object> list)
        {
            List<float> res = new List<float>();

            for (int i = 0; i < list.Count; i++)
            {
                res[i] = float.Parse(list[i].ToString());
            }

            return res;
        }
        internal static bool FloatEqual(float f0, float f1)
        {
            float f = Math.Abs(f0 - f1);

            return (f < 0.000000001f);
        }
    }
}