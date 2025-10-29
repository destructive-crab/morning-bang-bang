using Cysharp.Threading.Tasks;
using MothDIed.Pool;
using UnityEngine;

namespace banging_code.debug
{
    public class DebugLinesDrawer
    {
        private readonly DebuggerConfig debuggerConfig;
        private readonly BangDebugger bangDebugger;

        private readonly GameObjectPool<LineRenderer> linesPool;
        
        public DebugLinesDrawer(DebuggerConfig debuggerConfig, BangDebugger bangDebugger)
        {
            this.debuggerConfig = debuggerConfig;
            this.bangDebugger = bangDebugger;

            GameObjectPool<LineRenderer>.Config<LineRenderer> poolConfig = new (debuggerConfig.LinePrefab);
            
            poolConfig.Persistent = true;
            poolConfig.Expandable = true;
            poolConfig.SetFabric(new DebugPoolFabric());
            
            linesPool = new GameObjectPool<LineRenderer>(poolConfig);
        }

        public LineRenderer Draw(string name, Color color, float thickness, Vector3[] positions)
        {
            var newLine = linesPool.Pick();

            newLine.name = name;
            newLine.sortingOrder = 10;
            newLine.positionCount = positions.Length;
            newLine.SetPositions(positions);
            newLine.startColor = color;
            newLine.endColor = color;
            newLine.startWidth = thickness;
            newLine.endWidth = thickness;
            
            return newLine;
        }

        public void Clear(LineRenderer line)
        {
            if(line == null) return;
            linesPool.Release(line);
        }

        public void Clear()
        {
            linesPool.ReleaseAll();
        }

        public async UniTask Setup()
        {
            await linesPool.WarmAsync();
        }
    }
}