using MothDIed.Core.GameObjects.Pool;
using UnityEngine;

namespace banging_code.debug
{
    public class DebugLinesDrawer
    {
        private readonly DebuggerConfig debuggerConfig;
        private readonly BangDebugger debugger;

        private readonly GameObjectPool<LineRenderer> linesPool;
        
        public DebugLinesDrawer(DebuggerConfig debuggerConfig, BangDebugger debugger)
        {
            this.debuggerConfig = debuggerConfig;
            this.debugger = debugger;

            GameObjectPool<LineRenderer>.Config<LineRenderer> poolConfig = new (debuggerConfig.LinePrefab);
            
            poolConfig.Persistent = true;
            poolConfig.Expandable = true;
            poolConfig.Fabric = new DebugPoolFabric();
            
            linesPool = new GameObjectPool<LineRenderer>(poolConfig);
        }

        public void Draw(string name, Color color, float thickness, Vector3[] positions)
        {
            var newLine = linesPool.Get();

            newLine.name = name;
            newLine.transform.parent = debugger.GetDebugGOContainer();
            newLine.sortingOrder = 10;
            newLine.positionCount = positions.Length;
            newLine.SetPositions(positions);
            newLine.startColor = color;
            newLine.endColor = color;
            newLine.startWidth = thickness;
            newLine.endWidth = thickness;
        }

        public void Clear()
        {
            linesPool.ReleaseAll();
        }

        public void Setup()
        {
            linesPool.Warm();
        }
    }
}