using System.Collections.Generic;
using UnityEngine;

namespace banging_code.debug
{
    public class DebugLinesDrawer
    {
        private DebuggerConfig config;
        private BangDebugger debugger;

        private List<LineRenderer> lines = new();
        
        public DebugLinesDrawer(DebuggerConfig config, BangDebugger debugger)
        {
            this.config = config;
            this.debugger = debugger;
            
        }

        public void Draw(string name, Color color, float thickness, Vector3[] positions)
        {
            var newLine = NightPool.Spawn(config.LinePrefab);

            newLine.name = name;
            newLine.transform.parent = debugger.GetDebugGOContainer();
            newLine.sortingOrder = 10;
            newLine.positionCount = positions.Length;
            newLine.SetPositions(positions);
            newLine.startColor = color;
            newLine.endColor = color;
            newLine.startWidth = thickness;
            newLine.endWidth = thickness;

            lines.Add(newLine);
        }

        public void Clear()
        {
            foreach (var line in lines)
            {
                NightPool.Despawn(line.gameObject);
            }

            lines.Clear();
        }
    }
}