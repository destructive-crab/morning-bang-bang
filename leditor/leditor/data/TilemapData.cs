using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace leditor.root;

public class TilemapData
{
    public KeyValuePair<Vector2, string>[] Get => map.ToArray();
    private readonly Dictionary<Vector2, string> map = new();

    public TilemapData(KeyValuePair<Vector2, string>[] tiles)
    {
        foreach (KeyValuePair<Vector2,string> pair in tiles)
        {
            map.Add(pair.Key, pair.Value);
        }
    }
}