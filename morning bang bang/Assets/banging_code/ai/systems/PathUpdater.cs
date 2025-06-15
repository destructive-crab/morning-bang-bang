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

        public float PathUpdateRate = 0.1f;
        private Pathfinder pathfinder = new();
        private DebugLinesDrawer debugLinesDrawer;

        public override bool EnableOnStart() => true;
        public override void ContainerStarted()
        {
            debugLinesDrawer = new DebugLinesDrawer(Owner.transform);
        }

        public override void Enable()
        {
            base.Enable();
            Owner.StartCoroutine(UpdatePath());
        }

        private IEnumerator UpdatePath()
        {
            while (Enabled)
            {
                while (target.Target == null)
                {
                    yield return new WaitForEndOfFrame();
                }
                
                path.Path = pathfinder.FindPath(Owner.transform.position, target.Target.Position);

                if (Game.DebugFlags.ShowPaths && path.Path != null)
                {
                    debugLinesDrawer.Clear();
                    debugLinesDrawer.Draw("[PATH DEBUG]", Color.green, 0.2f, path.Path.Points);
                }
                else if(debugLinesDrawer.LinesCount > 0)
                {
                    debugLinesDrawer.Clear();
                }
                
                yield return new WaitForSeconds(PathUpdateRate);
            }
        }
    }
}