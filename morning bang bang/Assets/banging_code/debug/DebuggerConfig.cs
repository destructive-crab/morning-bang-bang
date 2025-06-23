using UnityEngine;

namespace banging_code.debug
{
    [CreateAssetMenu(fileName = "Debugger Config", menuName = "Debug")]
    public class DebuggerConfig : ScriptableObject
    {
        public LineRenderer LinePrefab;
        public DebugMapDrawer.DebugMapDrawerTiles DebugMapDrawerTiles;
    }
}