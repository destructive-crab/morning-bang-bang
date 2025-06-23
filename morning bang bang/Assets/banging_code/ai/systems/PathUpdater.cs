using System.Collections;
using banging_code.ai.pathfinding;
using banging_code.debug;
using MothDIed;
using MothDIed.DI;
using MothDIed.MonoSystems;
using UnityEngine;

namespace banging_code.ai.systems
{
    public class PathUpdater : MonoSystem, IEntityAISystem
    {
        [Inject] private EntityPath path;
        [Inject] private PathfinderTarget target;

        private Pathfinder pathfinder;
        private DebugLinesDrawer debugLinesDrawer;
        private LineRenderer currentDebugLine;

        public override bool EnableOnStart() => true;
        public override void ContainerStarted()
        {
        }

        public override void Enable()
        {
            base.Enable();
            pathfinder = new Pathfinder(Owner.GetComponent<DynamicObstacle>().ID);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Game.TryGetDebugger(out BangDebugger debugger))
            {
                debugger.Lines.Clear(currentDebugLine);
            }
        }

        public void UpdatePath()
        {
            path.Path = pathfinder.FindPath(Owner.transform.position, target.Target.Position);
            
            if (Game.TryGetDebugger(out BangDebugger debugger) && path != null && path.Path != null)
            {
                debugger.Lines.Clear(currentDebugLine);
                currentDebugLine = debugger.Lines.Draw("PATH", Color.magenta, 0.1f, path.Path.Points);
            }
        }
    }
}