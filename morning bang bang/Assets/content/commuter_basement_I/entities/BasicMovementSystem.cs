using System.Collections;
using banging_code.ai.pathfinding;
using banging_code.ai.targeting;
using banging_code.common;
using banging_code.dev;
using MothDIed.DI;
using MothDIed.ExtensionSystem;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BasicMovementSystem : Extension
    {
        [Inject] private TargetSelector targetSelector;
        [Inject] private Rigidbody2D rigidbody;
        [Inject] private Flipper flipper;

        public float Speed = 7;
        
        private readonly Pathfinder pathfinder = new();
        private Pathfinder.Path currentPath;

        private const float PATH_UPDATE_RATE = 0.01f;
        private Coroutine pathUpdateCoroutine;

        private LinesDrawer linesDrawer;

        public override void StartExtension()
        {
            linesDrawer = new LinesDrawer(Owner.transform);
        }

        public void StartMoving()
        {
            if(targetSelector.BestTarget == null)
            {
                Debug.LogWarning("[BASIC ENTITY TO TARGET MOVEMENT : START MOVING] CAN NOT START MOVING WHEN THERE IS NO TARGET");
                return;
            }

            currentPath = pathfinder.FindPath(Owner.transform.position, targetSelector.BestTarget.Position);
            pathUpdateCoroutine = Owner.StartCoroutine(UpdatePath());
        }

        public void StopMoving()
        {
            if (currentPath == null)
            {                
                Debug.LogWarning("[BASIC ENTITY TO TARGET MOVEMENT : STOP MOVING] ENTITY IS NOT MOVING");
                return;
            }
            
            Owner.StopCoroutine(pathUpdateCoroutine);
            currentPath = null;
        }

        public override void Update()
        {
            if (currentPath != null && !currentPath.Completed)
            {
                if(flipper!=null) flipper.FaceToPoint(currentPath.CurrentPoint);
                rigidbody.MovePosition(Vector3.MoveTowards(rigidbody.position, currentPath.CurrentPoint, Time.deltaTime *  Speed));

                if (Vector3.Distance(rigidbody.position, currentPath.CurrentPoint) < 0.01f)
                {
                    currentPath.Next();
                }
            }
        }

        private IEnumerator UpdatePath()
        {
            while (Owner != null && Owner.enabled)
            {
                yield return new WaitForSeconds(PATH_UPDATE_RATE);
                
                if(targetSelector.BestTarget == null)
                {
                    currentPath = null;
                    continue;
                }

                currentPath = pathfinder.FindPath(Owner.transform.position, targetSelector.BestTarget.Position);

                if (currentPath == null) continue;
                
                linesDrawer.Clear();
                linesDrawer.Draw(Color.green, 0.1f, new []{Owner.transform.position, currentPath.Points[0]});
                linesDrawer.Draw(Color.green, 0.1f, currentPath.Points);
            }
        }
        
    }
}