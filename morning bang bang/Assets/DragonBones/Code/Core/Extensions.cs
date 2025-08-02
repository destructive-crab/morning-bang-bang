using System.Collections.Generic;

namespace DragonBones
{
    internal static class Extensions
    {
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
    }
}