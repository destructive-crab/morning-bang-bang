using System.Collections;
using banging_code.ai.pathfinding;
using MothDIed.DI;
using MothDIed.MonoSystems;
using UnityEngine;

namespace banging_code.ai.systems
{
    public class PathUpdater : MonoSystem
    {
        [Inject] private EntityPath path;
        [Inject] private PathfinderTarget target;

        public float PathUpdateRate = 0.1f;
        private Pathfinder pathfinder = new();

        public override bool EnableOnStart()
        {
            return false;
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
                path.Path = pathfinder.FindPath(Owner.transform.position, target.Target.Position);
                yield return new WaitForSeconds(PathUpdateRate);
            }
        }
    }
}