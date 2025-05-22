using System.Collections;
using banging_code.ai.pathfinding;
using banging_code.dev;
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
        private LinesDrawer linesDrawer;

        public override bool EnableOnStart() => true;
        public override void ContainerStarted()
        {
            linesDrawer = new LinesDrawer(Owner.transform);
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
                    linesDrawer.Clear();
                    linesDrawer.Draw("[PATH DEBUG]", Color.green, 0.2f, path.Path.Points);
                }
                else if(linesDrawer.LinesCount > 0)
                {
                    linesDrawer.Clear();
                }
                
                yield return new WaitForSeconds(PathUpdateRate);
            }
        }
    }
}