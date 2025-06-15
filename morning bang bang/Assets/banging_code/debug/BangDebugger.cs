using UnityEngine;

namespace banging_code.debug
{
    public sealed class BangDebugger
    {
        private readonly DebuggerConfig debuggerConfig;
        
        public BangingConsole Console;
        
        public DebugLinesDrawer Lines;
        public DebugMapDrawer Map;

        private Transform container;

        public BangDebugger(DebuggerConfig config)
        {
            debuggerConfig = config;
        }

        public Transform GetDebugGOContainer()
        {
            if (container == null)
            {
                container = new GameObject("[DEBUG GAME OBJECT CONTAINER]").transform;
            }

            return container;
        }

        public void SetupDebugger()
        {
            Lines = new DebugLinesDrawer(debuggerConfig, this);
            Lines.Setup();
        }
    }
}